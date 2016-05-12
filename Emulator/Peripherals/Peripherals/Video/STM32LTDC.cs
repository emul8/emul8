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
            lock_obj = new object();

            activeWidthConfigurationRegister = new DoubleWordRegister(this);
            accumulatedActiveHeightField = activeWidthConfigurationRegister.DefineValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "AAH");
            accumulatedActiveWidthField = activeWidthConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "AAW", writeCallback: (_, __) => HandleActiveDisplayChange());

            backPorchConfigurationRegister = new DoubleWordRegister(this);
            accumulatedVerticalBackPorchField = backPorchConfigurationRegister.DefineValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "AVBP");
            accumulatedHorizontalBackPorchField = backPorchConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "AHBP", writeCallback: (_, __) => HandleActiveDisplayChange());

            backgroundColorConfigurationRegister = new DoubleWordRegister(this);
            backgroundColorBlueChannelField = backgroundColorConfigurationRegister.DefineValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "BCBLUE");
            backgroundColorGreenChannelField = backgroundColorConfigurationRegister.DefineValueField(8, 8, FieldMode.Read | FieldMode.Write, name: "BCGREEN");
            backgroundColorRedChannelField = backgroundColorConfigurationRegister.DefineValueField(16, 8, FieldMode.Read | FieldMode.Write, name: "BCRED", writeCallback: (_, __) => HandleBackgroundColorChange());

            interruptEnableRegister = new DoubleWordRegister(this);
            lineInterruptEnableFlag = interruptEnableRegister.DefineFlagField(0, FieldMode.Read | FieldMode.Write, name: "LIE");

            interruptStatusRegister = new DoubleWordRegister(this);
            interruptStatusRegister.DefineFlagField(0, FieldMode.Read, name: "LIF");

            interruptClearRegister = new DoubleWordRegister(this);
            interruptClearRegister.DefineFlagField(0, FieldMode.Write, name: "CLIF", writeCallback: (old, @new) => { if(@new) IRQ.Unset(); });
            interruptClearRegister.DefineFlagField(3, FieldMode.Write, name: "CRRIF", writeCallback: (old, @new) => { if(@new) IRQ.Unset(); });

            lineInterruptPositionConfigurationRegister = new DoubleWordRegister(this).WithValueField(0, 11, FieldMode.Read | FieldMode.Write, name: "LIPOS");

            var registerMappings = new Dictionary<long, DoubleWordRegister>
            {
                { 0x0C, backPorchConfigurationRegister },
                { 0x10, activeWidthConfigurationRegister },
                { 0x2C, backgroundColorConfigurationRegister },
                { 0x34, interruptEnableRegister },
                { 0x38, interruptStatusRegister },
                { 0x3C, interruptClearRegister },
                { 0x40, lineInterruptPositionConfigurationRegister }
            };

            layer = new Layer[2];
            for(int i = 0; i < layer.Length; i++)
            {
                layer[i] = new Layer(this, i);

                registerMappings.Add(0x84 + 0x80 * i, layer[i].controlRegister);
                registerMappings.Add(0x88 + 0x80 * i, layer[i].windowHorizontalPositionConfigurationRegister);
                registerMappings.Add(0x8C + 0x80 * i, layer[i].windowVerticalPositionConfigurationRegister);
                registerMappings.Add(0x94 + 0x80 * i, layer[i].pixelFormatConfigurationRegister);
                registerMappings.Add(0x98 + 0x80 * i, layer[i].constantAlphaConfigurationRegister);
                registerMappings.Add(0xAC + 0x80 * i, layer[i].colorFrameBufferAddressRegister);
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
            lock(lock_obj)
            {
                if(Width == 0 || Height == 0)
                {
                    return;
                }

                var localLayerBuffer = new byte[2][];

                for(int i = 0; i < 2; i++)
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
            lock(lock_obj)
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
            lock(lock_obj)
            {
                blender = PixelManipulationTools.GetBlender(layer[0].pixelFormatField.Value.ToPixelFormat(), Endianess, layer[1].pixelFormatField.Value.ToPixelFormat(), Endianess, Format, Endianess);
            }
        }

        private readonly DoubleWordRegister backPorchConfigurationRegister;
        private readonly IValueRegisterField accumulatedVerticalBackPorchField;
        private readonly IValueRegisterField accumulatedHorizontalBackPorchField;

        private readonly DoubleWordRegister activeWidthConfigurationRegister;
        private readonly IValueRegisterField accumulatedActiveHeightField;
        private readonly IValueRegisterField accumulatedActiveWidthField;
        private readonly DoubleWordRegister interruptStatusRegister;
        private readonly DoubleWordRegister interruptClearRegister;
        private readonly DoubleWordRegister backgroundColorConfigurationRegister;
        private readonly IValueRegisterField backgroundColorBlueChannelField;
        private readonly IValueRegisterField backgroundColorGreenChannelField;
        private readonly IValueRegisterField backgroundColorRedChannelField;
        private readonly DoubleWordRegister interruptEnableRegister;
        private readonly IFlagRegisterField lineInterruptEnableFlag;
        private readonly DoubleWordRegister lineInterruptPositionConfigurationRegister;
        private readonly Layer[] layer;
        private readonly DoubleWordRegisterCollection registers;

        private readonly object lock_obj;
        private readonly Machine machine;

        private IPixelBlender blender;
        private Pixel backgroundColor;

        private class Layer
        {
            public Layer(STM32LTDC video, int i)
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

                layerId = i;
                this.video = video;
            }

            public void RestoreBuffers()
            {
                lock(video.lock_obj)
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
                lock(video.lock_obj)
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
                lock(video.lock_obj)
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
                for(int j = 0; j < colorBuffer.Length; j += 4)
                {
                    colorBuffer[j] = (byte)defaultColorAlphaField.Value;
                    colorBuffer[j + 1] = (byte)defaultColorRedField.Value;
                    colorBuffer[j + 2] = (byte)defaultColorGreenField.Value;
                    colorBuffer[j + 3] = (byte)defaultColorBlueField.Value;
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
