//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Backends.Display;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Core.Structure.Registers;
using System.Collections.Generic;
using Emul8.Peripherals.DMA;
using Emul8.Logging;

namespace Emul8.Peripherals.Video
{
    public class STM32LTDC : AutoRepaintingVideo, IDoubleWordPeripheral, IKnownSize
    {
        public STM32LTDC(Machine machine) : base(machine)
        {
            Reconfigure(format: PixelFormat.RGBX8888);

            IRQ = new GPIO();

            this.machine = machine;
            internalLock = new object();

            var activeWidthConfigurationRegister = new DoubleWordRegister(this);
            accumulatedActiveHeightField = activeWidthConfigurationRegister.DefineValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "AAH");
            accumulatedActiveWidthField = activeWidthConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "AAW", writeCallback: (_, __) => HandleActiveDisplayChange());

            var backPorchConfigurationRegister = new DoubleWordRegister(this);
            accumulatedVerticalBackPorchField = backPorchConfigurationRegister.DefineValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "AVBP");
            accumulatedHorizontalBackPorchField = backPorchConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "AHBP", writeCallback: (_, __) => HandleActiveDisplayChange());

            var backgroundColorConfigurationRegister = new DoubleWordRegister(this);
            backgroundColorBlueChannelField = backgroundColorConfigurationRegister.DefineValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "BCBLUE");
            backgroundColorGreenChannelField = backgroundColorConfigurationRegister.DefineValueField(8, 8, FieldMode.Read | FieldMode.Write, name: "BCGREEN");
            backgroundColorRedChannelField = backgroundColorConfigurationRegister.DefineValueField(16, 8, FieldMode.Read | FieldMode.Write, name: "BCRED", writeCallback: (_, __) => HandleBackgroundColorChange());

            var interruptEnableRegister = new DoubleWordRegister(this);
            lineInterruptEnableFlag = interruptEnableRegister.DefineFlagField(0, FieldMode.Read | FieldMode.Write, name: "LIE");

            var interruptClearRegister = new DoubleWordRegister(this);
            interruptClearRegister.DefineFlagField(0, FieldMode.Write, name: "CLIF", writeCallback: (old, @new) => { if(@new) IRQ.Unset(); });
            interruptClearRegister.DefineFlagField(3, FieldMode.Write, name: "CRRIF", writeCallback: (old, @new) => { if(@new) IRQ.Unset(); });

            lineInterruptPositionConfigurationRegister = new DoubleWordRegister(this).WithValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "LIPOS");

            var registerMappings = new Dictionary<long, DoubleWordRegister>
            {
                { (long)Register.BackPorchConfigurationRegister, backPorchConfigurationRegister },
                { (long)Register.ActiveWidthConfigurationRegister, activeWidthConfigurationRegister },
                { (long)Register.BackgroundColorConfigurationRegister, backgroundColorConfigurationRegister },
                { (long)Register.InterruptEnableRegister, interruptEnableRegister },
                { (long)Register.InterruptClearRegister, interruptClearRegister },
                { (long)Register.LineInterruptPositionConfigurationRegister, lineInterruptPositionConfigurationRegister }
            };

            layer = new Layer[2];
            for(var i = 0; i < layer.Length; i++)
            {
                layer[i] = new Layer(this, i);

                var offset = 0x80 * i;
                registerMappings.Add(0x84 + offset, layer[i].controlRegister);
                registerMappings.Add(0x88 + offset, layer[i].windowHorizontalPositionConfigurationRegister);
                registerMappings.Add(0x8C + offset, layer[i].windowVerticalPositionConfigurationRegister);
                registerMappings.Add(0x94 + offset, layer[i].pixelFormatConfigurationRegister);
                registerMappings.Add(0x98 + offset, layer[i].constantAlphaConfigurationRegister);
                registerMappings.Add(0xAC + offset, layer[i].colorFrameBufferAddressRegister);
            }

            registers = new DoubleWordRegisterCollection(this, registerMappings);
            registers.Reset();
            HandlePixelFormatChange();
        }

        public GPIO IRQ { get; private set; }

        public long Size { get { return 0xC00; } }

        public void WriteDoubleWord(long address, uint value)
        {
            registers.Write(address, value);
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public override void Reset()
        {
            registers.Reset();
        }

        protected override void Repaint()
        {
            lock(internalLock)
            {
                if(Width == 0 || Height == 0)
                {
                    return;
                }

                var localLayerBuffer = new byte[2][];

                for(var i = 0; i < 2; i++)
                {
                    if(layer[i].layerEnableFlag.Value && layer[i].colorFrameBufferAddressRegister.Value != 0)
                    {
                        machine.SystemBus.ReadBytes(layer[i].colorFrameBufferAddressRegister.Value, layer[i].layerBuffer.Length, layer[i].layerBuffer, 0);
                        localLayerBuffer[i] = layer[i].layerBuffer;
                    }
                    else
                    {
                        localLayerBuffer[i] = layer[i].layerBackgroundBuffer;                
                    }
                }

                blender.Blend(localLayerBuffer[0], localLayerBuffer[1], ref buffer, backgroundColor, (byte)layer[0].constantAlphaConfigurationRegister.Value, (byte)layer[1].constantAlphaConfigurationRegister.Value);

                if(lineInterruptEnableFlag.Value)
                {
                    IRQ.Set();
                }
            }
        }

        private void HandleBackgroundColorChange()
        {
            backgroundColor = new Pixel(
                (byte)backgroundColorRedChannelField.Value, 
                (byte)backgroundColorGreenChannelField.Value, 
                (byte)backgroundColorBlueChannelField.Value, 
                (byte)0xFF);
        }

        private void HandleActiveDisplayChange()
        {
            lock(internalLock)
            {
                var width = (int)(accumulatedActiveWidthField.Value - accumulatedHorizontalBackPorchField.Value);
                var height = (int)(accumulatedActiveHeightField.Value - accumulatedVerticalBackPorchField.Value);

                if((width == Width && height == Height) || width < 0 || height < 0)
                {
                    return;
                }

                Reconfigure(width, height);
                layer[0].RestoreBuffers();
                layer[1].RestoreBuffers();
            }
        }

        private void HandlePixelFormatChange()
        {
            lock(internalLock)
            {
                blender = PixelManipulationTools.GetBlender(layer[0].pixelFormatField.Value.ToPixelFormat(), Endianess, layer[1].pixelFormatField.Value.ToPixelFormat(), Endianess, Format, Endianess);
            }
        }

        private readonly IValueRegisterField accumulatedVerticalBackPorchField;
        private readonly IValueRegisterField accumulatedHorizontalBackPorchField;
        private readonly IValueRegisterField accumulatedActiveHeightField;
        private readonly IValueRegisterField accumulatedActiveWidthField;
        private readonly IValueRegisterField backgroundColorBlueChannelField;
        private readonly IValueRegisterField backgroundColorGreenChannelField;
        private readonly IValueRegisterField backgroundColorRedChannelField;
        private readonly IFlagRegisterField lineInterruptEnableFlag;
        private readonly DoubleWordRegister lineInterruptPositionConfigurationRegister;
        private readonly Layer[] layer;
        private readonly DoubleWordRegisterCollection registers;

        private readonly object internalLock;
        private readonly Machine machine;

        private IPixelBlender blender;
        private Pixel backgroundColor;

        private enum Register : long
        {
            BackPorchConfigurationRegister = 0x0C,
            ActiveWidthConfigurationRegister = 0x10,
            BackgroundColorConfigurationRegister = 0x2C,
            InterruptEnableRegister = 0x34,
            InterruptClearRegister = 0x3C,
            LineInterruptPositionConfigurationRegister = 0x40,
        }

        private class Layer
        {
            public Layer(STM32LTDC video, int layerId)
            {
                controlRegister = new DoubleWordRegister(video);
                layerEnableFlag = controlRegister.DefineFlagField(0, FieldMode.Read | FieldMode.Write, name: "LEN", writeCallback: (_, __) => WarnAboutWrongBufferConfiguration());

                windowHorizontalPositionConfigurationRegister = new DoubleWordRegister(video);
                windowHorizontalStartPositionField = windowHorizontalPositionConfigurationRegister.DefineValueField(0, 12, FieldMode.Read | FieldMode.Write, name: "WHSTPOS");
                windowHorizontalStopPositionField = windowHorizontalPositionConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "WHSPPOS", writeCallback: (_, __) => HandleLayerWindowConfigurationChange());

                windowVerticalPositionConfigurationRegister = new DoubleWordRegister(video);
                windowVerticalStartPositionField = windowVerticalPositionConfigurationRegister.DefineValueField(0, 12, FieldMode.Read | FieldMode.Write, name: "WVSTPOS");
                windowVerticalStopPositionField = windowVerticalPositionConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "WVSPPOS", writeCallback: (_, __) => HandleLayerWindowConfigurationChange());

                pixelFormatConfigurationRegister = new DoubleWordRegister(video);
                pixelFormatField = pixelFormatConfigurationRegister.DefineEnumField<Dma2DColorMode>(0, 3, FieldMode.Read | FieldMode.Write, name: "PF", writeCallback: (_, __) => { RestoreBuffers(); video.HandlePixelFormatChange(); });

                constantAlphaConfigurationRegister = new DoubleWordRegister(video, 0xFF).WithValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "CONSTA");

                colorFrameBufferAddressRegister = new DoubleWordRegister(video).WithValueField(0, 32, FieldMode.Read | FieldMode.Write, name: "CFBADD", writeCallback: (_, __) => WarnAboutWrongBufferConfiguration());

                defaultColorConfigurationRegister = new DoubleWordRegister(video);
                defaultColorBlueField = defaultColorConfigurationRegister.DefineValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "DCBLUE");
                defaultColorGreenField = defaultColorConfigurationRegister.DefineValueField(8, 8, FieldMode.Read | FieldMode.Write, name: "DCGREEN");
                defaultColorRedField = defaultColorConfigurationRegister.DefineValueField(16, 8, FieldMode.Read | FieldMode.Write, name: "DCRED");
                defaultColorAlphaField = defaultColorConfigurationRegister.DefineValueField(24, 8, FieldMode.Read | FieldMode.Write, name: "DCALPHA", writeCallback: (_, __) => HandleLayerBackgroundColorChange());

                this.layerId = layerId;
                this.video = video;
            }

            public void RestoreBuffers()
            {
                lock(video.internalLock)
                {
                    var layerPixelFormat = pixelFormatField.Value.ToPixelFormat();
                    var colorDepth = layerPixelFormat.GetColorDepth();
                    layerBuffer = new byte[video.Width * video.Height * colorDepth];
                    layerBackgroundBuffer = new byte[layerBuffer.Length];

                    HandleLayerBackgroundColorChange();
                }
            }

            private void WarnAboutWrongBufferConfiguration()
            {
                lock(video.internalLock)
                {
                    if(layerEnableFlag.Value && colorFrameBufferAddressRegister.Value == 0)
                    {
                        if(!warningFlag)
                        {
                            video.Log(LogLevel.Warning, "Layer {0} is enabled, but no frame buffer register is set", layerId);
                            warningFlag = true;
                        }
                    }
                    else
                    {
                        warningFlag = false;
                    }
                }
            }

            private void HandleLayerWindowConfigurationChange()
            {
                lock(video.internalLock)
                {
                    var width = (int)(windowHorizontalStopPositionField.Value - windowHorizontalStartPositionField.Value) + 1;
                    var height = (int)(windowVerticalStopPositionField.Value - windowVerticalStartPositionField.Value) + 1;

                    if(width != video.Width || height != video.Height)
                    {
                        video.Log(LogLevel.Warning, "Windowing is not supported yet for layer {0}.", layerId);
                    }
                }
            }

            private void HandleLayerBackgroundColorChange()
            {
                var colorBuffer = new byte[4 * video.Width * video.Height];
                for(var i = 0; i < colorBuffer.Length; i += 4)
                {
                    colorBuffer[i] = (byte)defaultColorAlphaField.Value;
                    colorBuffer[i + 1] = (byte)defaultColorRedField.Value;
                    colorBuffer[i + 2] = (byte)defaultColorGreenField.Value;
                    colorBuffer[i + 3] = (byte)defaultColorBlueField.Value;
                }

                PixelManipulationTools.GetConverter(PixelFormat.ARGB8888, video.Endianess, pixelFormatField.Value.ToPixelFormat(), video.Endianess)
                                      .Convert(colorBuffer, ref layerBackgroundBuffer);
            }

            public DoubleWordRegister controlRegister;
            public IFlagRegisterField layerEnableFlag;

            public DoubleWordRegister pixelFormatConfigurationRegister;
            public IEnumRegisterField<Dma2DColorMode> pixelFormatField;

            public DoubleWordRegister constantAlphaConfigurationRegister;

            public DoubleWordRegister colorFrameBufferAddressRegister;

            public DoubleWordRegister windowHorizontalPositionConfigurationRegister;
            public IValueRegisterField windowHorizontalStopPositionField;
            public IValueRegisterField windowHorizontalStartPositionField;

            public DoubleWordRegister windowVerticalPositionConfigurationRegister;
            public IValueRegisterField windowVerticalStopPositionField;
            public IValueRegisterField windowVerticalStartPositionField;

            public DoubleWordRegister defaultColorConfigurationRegister;
            public IValueRegisterField defaultColorBlueField;
            public IValueRegisterField defaultColorGreenField;
            public IValueRegisterField defaultColorRedField;
            public IValueRegisterField defaultColorAlphaField;

            public byte[] layerBuffer;
            public byte[] layerBackgroundBuffer;

            private bool warningFlag;
            private readonly int layerId;
            private readonly STM32LTDC video;
        }
    }
}
