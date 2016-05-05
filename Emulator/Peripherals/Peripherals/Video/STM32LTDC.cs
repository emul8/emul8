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

namespace Emul8.Peripherals.Video
{
    public class STM32LTDC : AutoRepaintingVideo, IDoubleWordPeripheral, IKnownSize
    {
        public STM32LTDC(Machine machine) : base(machine)
        {
            Reconfigure(format: PixelFormat.RGBX8888);

            IRQ = new GPIO();

            layerBuffer = new byte[2][];
            layerBackgroundBuffer = new byte[2][];
            this.machine = machine;
            lock_obj = new object();

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
                { 0x2C, backgroundColorConfigurationRegister },
                { 0x34, interruptEnableRegister },
                { 0x38, interruptStatusRegister },
                { 0x3C, interruptClearRegister },
                { 0x40, lineInterruptPositionConfigurationRegister }
            };

            layer = new LayerRegisters[2];
            for(int i = 0; i < layer.Length; i++)
            {
                layer[i] = new LayerRegisters(this, i);

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
                    if(layer[i].layerEnableFlag.Value)
                    {
                        machine.SystemBus.ReadBytes(layer[i].colorFrameBufferAddressRegister.Value, layerBuffer[i].Length, layerBuffer[i], 0);
                        localLayerBuffer[i] = layerBuffer[i];
                    }
                    else
                    {
                        localLayerBuffer[i] = layerBackgroundBuffer[i];                
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

        private void HandleLayerBackgroundColorChange(int layerId)
        {
            var colorBuffer = new byte[4 * Width * Height];
            for(int j = 0; j < colorBuffer.Length; j += 4)
            {
                colorBuffer[j] = (byte)layer[layerId].defaultColorAlphaField.Value;
                colorBuffer[j + 1] = (byte)layer[layerId].defaultColorRedField.Value;
                colorBuffer[j + 2] = (byte)layer[layerId].defaultColorGreenField.Value;
                colorBuffer[j + 3] = (byte)layer[layerId].defaultColorBlueField.Value;
            }

            PixelManipulationTools.GetConverter(PixelFormat.ARGB8888, Endianess, layer[layerId].pixelFormatField.Value.ToPixelFormat(), Endianess)
                                  .Convert(colorBuffer, ref layerBackgroundBuffer[layerId]);
        }

        private void HandlePositionConfigurationChange(int layerId)
        {
            lock(lock_obj)
            {
                var width = (int)(layer[layerId].windowHorizontalStopPositionField.Value - layer[layerId].windowHorizontalStartPositionField.Value) + 1;
                var height = (int)(layer[layerId].windowVerticalStopPositionField.Value - layer[layerId].windowVerticalStartPositionField.Value) + 1;
                if(width != 0 && height != 0 && (width != Width || height != Height))
                {
                    Reconfigure(width, height);
                    RestoreBuffer(layerId);
                }
            }
        }

        private void HandlePixelFormatChange()
        {
            lock(lock_obj)
            {
                blender = PixelManipulationTools.GetBlender(layer[0].pixelFormatField.Value.ToPixelFormat(), Endianess, layer[1].pixelFormatField.Value.ToPixelFormat(), Endianess, Format, Endianess);
            }
        }

        private void RestoreBuffer(int layerId)
        {
            lock(lock_obj)
            {
                var layerPixelFormat = layer[layerId].pixelFormatField.Value.ToPixelFormat();
                layerBuffer[layerId] = new byte[Width * Height * layerPixelFormat.GetColorDepth()];
                layerBackgroundBuffer[layerId] = new byte[Width * Height * layerPixelFormat.GetColorDepth()];

                HandleLayerBackgroundColorChange(layerId);
            }
        }

        private readonly DoubleWordRegister interruptStatusRegister;
        private readonly DoubleWordRegister interruptClearRegister;
        private readonly DoubleWordRegister backgroundColorConfigurationRegister;
        private readonly IValueRegisterField backgroundColorBlueChannelField;
        private readonly IValueRegisterField backgroundColorGreenChannelField;
        private readonly IValueRegisterField backgroundColorRedChannelField;
        private readonly DoubleWordRegister interruptEnableRegister;
        private readonly IFlagRegisterField lineInterruptEnableFlag;
        private readonly DoubleWordRegister lineInterruptPositionConfigurationRegister;
        private readonly LayerRegisters[] layer;
        private readonly DoubleWordRegisterCollection registers;

        private readonly object lock_obj;
        private readonly Machine machine;
        private readonly byte[][] layerBuffer;
        private readonly byte[][] layerBackgroundBuffer;

        private IPixelBlender blender;
        private Pixel backgroundColor;

        private struct LayerRegisters
        {
            public LayerRegisters(STM32LTDC video, int i)
            {
                controlRegister = new DoubleWordRegister(video);
                layerEnableFlag = controlRegister.DefineFlagField(0, FieldMode.Read | FieldMode.Write, name: "LEN");

                windowHorizontalPositionConfigurationRegister = new DoubleWordRegister(video);
                windowHorizontalStartPositionField = windowHorizontalPositionConfigurationRegister.DefineValueField(0, 12, FieldMode.Read | FieldMode.Write, name: "WHSTPOS");
                windowHorizontalStopPositionField = windowHorizontalPositionConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "WHSPPOS", writeCallback: (_, __) => video.HandlePositionConfigurationChange(i));

                windowVerticalPositionConfigurationRegister = new DoubleWordRegister(video);
                windowVerticalStartPositionField = windowVerticalPositionConfigurationRegister.DefineValueField(0, 12, FieldMode.Read | FieldMode.Write, name: "WVSTPOS");
                windowVerticalStopPositionField = windowVerticalPositionConfigurationRegister.DefineValueField(16, 12, FieldMode.Read | FieldMode.Write, name: "WVSPPOS", writeCallback: (_, __) => video.HandlePositionConfigurationChange(i));

                pixelFormatConfigurationRegister = new DoubleWordRegister(video);
                pixelFormatField = pixelFormatConfigurationRegister.DefineEnumField<Dma2DColorMode>(0, 3, FieldMode.Read | FieldMode.Write, name: "PF", writeCallback: (_, __) => { video.RestoreBuffer(i); video.HandlePixelFormatChange(); });

                constantAlphaConfigurationRegister = new DoubleWordRegister(video, 0xFF).WithValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "CONSTA");

                colorFrameBufferAddressRegister = new DoubleWordRegister(video).WithValueField(0, 32, FieldMode.Read | FieldMode.Write, name: "CFBADD");

                defaultColorConfigurationRegister = new DoubleWordRegister(video);
                defaultColorBlueField = defaultColorConfigurationRegister.DefineValueField(0, 8, FieldMode.Read | FieldMode.Write, name: "DCBLUE");
                defaultColorGreenField = defaultColorConfigurationRegister.DefineValueField(8, 8, FieldMode.Read | FieldMode.Write, name: "DCGREEN");
                defaultColorRedField = defaultColorConfigurationRegister.DefineValueField(16, 8, FieldMode.Read | FieldMode.Write, name: "DCRED");
                defaultColorAlphaField = defaultColorConfigurationRegister.DefineValueField(24, 8, FieldMode.Read | FieldMode.Write, name: "DCALPHA", writeCallback: (_, __) => video.HandleLayerBackgroundColorChange(i));
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
        }
    }
}
