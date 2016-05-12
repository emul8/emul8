//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Peripherals.Bus;
using Emul8.Core;
using System.Collections.Generic;
using Emul8.Backends.Display;
using Emul8.Core.Structure.Registers;
using System;

namespace Emul8.Peripherals.DMA
{
    public sealed class STM32DMA2D : IDoubleWordPeripheral, IKnownSize
    {
        public STM32DMA2D(Machine machine) : this()
        {
            this.machine = machine;
            IRQ = new GPIO();
            Reset();
        }

        public void Reset()
        {
            registers.Reset();
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public GPIO IRQ { get; private set; }

        public long Size
        {
            get
            {
                return 0xC00;
            }
        }

        private STM32DMA2D()
        {
            controlRegister = new DoubleWordRegister(this);
            startFlag = controlRegister.DefineFlagField(0, FieldMode.Read | FieldMode.Write, name: "Start", changeCallback: (old, @new) => { if(@new) StartTransfer(); });
            dma2dMode = controlRegister.DefineEnumField<Mode>(16, 2, FieldMode.Read | FieldMode.Write, name: "Mode");

            interruptFlagClearRegister = new DoubleWordRegister(this).WithFlag(1, FieldMode.Read | FieldMode.WriteOneToClear, name: "CTCIF", changeCallback: (old, @new) => { if(!@new) IRQ.Unset(); });

            numberOfLineRegister = new DoubleWordRegister(this);
            numberOfLineField = numberOfLineRegister.DefineValueField(0, 16, FieldMode.Read | FieldMode.Write, name: "NL");
            pixelsPerLineField = numberOfLineRegister.DefineValueField(16, 14, FieldMode.Read | FieldMode.Write, name: "PL");

            outputMemoryAddressRegister = new DoubleWordRegister(this).WithValueField(0, 32, FieldMode.Read | FieldMode.Write);
            backgroundMemoryAddressRegister = new DoubleWordRegister(this).WithValueField(0, 32, FieldMode.Read | FieldMode.Write);
            foregroundMemoryAddressRegister = new DoubleWordRegister(this).WithValueField(0, 32, FieldMode.Read | FieldMode.Write);

            outputPfcControlRegister = new DoubleWordRegister(this);
            outputColorModeField = outputPfcControlRegister.DefineEnumField<Dma2DColorMode>(0, 3, FieldMode.Read | FieldMode.Write, name: "CM");

            foregroundPfcControlRegister = new DoubleWordRegister(this);
            foregroundColorModeField = foregroundPfcControlRegister.DefineEnumField<Dma2DColorMode>(0, 4, FieldMode.Read | FieldMode.Write, name: "CM");

            backgroundPfcControlRegister = new DoubleWordRegister(this);
            backgroundColorModeField = backgroundPfcControlRegister.DefineEnumField<Dma2DColorMode>(0, 4, FieldMode.Read | FieldMode.Write, name: "CM");

            outputColorRegister = new DoubleWordRegister(this).WithValueField(0, 32, FieldMode.Read | FieldMode.Write);

            outputOffsetRegister = new DoubleWordRegister(this);
            outputLineOffsetField = outputOffsetRegister.DefineValueField(0, 14, FieldMode.Read | FieldMode.Write, name: "LO");

            foregroundOffsetRegister = new DoubleWordRegister(this);
            foregroundLineOffsetField = foregroundOffsetRegister.DefineValueField(0, 14, FieldMode.Read | FieldMode.Write, name: "LO");

            backgroundOffsetRegister = new DoubleWordRegister(this);
            backgroundLineOffsetField = backgroundOffsetRegister.DefineValueField(0, 14, FieldMode.Read | FieldMode.Write, name: "LO");

            var regs = new Dictionary<long, DoubleWordRegister>
            {
                { 0x00, controlRegister },
                { 0x08, interruptFlagClearRegister },
                { 0x0C, foregroundMemoryAddressRegister },
                { 0x10, foregroundOffsetRegister },
                { 0x14, backgroundMemoryAddressRegister },
                { 0x18, backgroundOffsetRegister },
                { 0x1C, foregroundPfcControlRegister },
                { 0x24, backgroundPfcControlRegister },
                { 0x34, outputPfcControlRegister },
                { 0x38, outputColorRegister },
                { 0x3C, outputMemoryAddressRegister },
                { 0x40, outputOffsetRegister },
                { 0x44, numberOfLineRegister }
            };

            registers = new DoubleWordRegisterCollection(this, regs);
        }

        private void StartTransfer()
        {
            var foregroundFormat = foregroundColorModeField.Value.ToPixelFormat();
            var outputFormat = outputColorModeField.Value.ToPixelFormat();

            byte[] outputBuffer;
            switch(dma2dMode.Value)
            {
                case Mode.RegisterToMemory:
                    var colorBytes = BitConverter.GetBytes(outputColorRegister.Value);
                    var colorDepth = outputFormat.GetColorDepth();

                    outputBuffer = new byte[numberOfLineField.Value * pixelsPerLineField.Value * colorDepth];
                    // fill area with the color defined in output color register
                    for(int i = 0; i < outputBuffer.Length; i++)
                    {
                        outputBuffer[i] = colorBytes[i % colorDepth];
                    }

                    if(outputLineOffsetField.Value == 0)
                    {
                        // we can copy everything at once - it might be faster
                        machine.SystemBus.WriteBytes(outputBuffer, outputMemoryAddressRegister.Value);
                    }
                    else
                    {
                        // we have to copy per line
                        var lineWidth = (int)(pixelsPerLineField.Value * outputFormat.GetColorDepth());
                        var offset = lineWidth + (outputLineOffsetField.Value * outputFormat.GetColorDepth());
                        for(int line = 0; line < numberOfLineField.Value; line++)
                        {
                            machine.SystemBus.WriteBytes(outputBuffer, outputMemoryAddressRegister.Value + line * offset, line * lineWidth, lineWidth);
                        }
                    }
                break;
                case Mode.MemoryToMemoryWithBlending:
                    var backgroundFormat = backgroundColorModeField.Value.ToPixelFormat();
                    var blender = PixelManipulationTools.GetBlender(backgroundFormat, ENDIANESS, foregroundFormat, ENDIANESS, outputFormat, ENDIANESS);

                    if(outputLineOffsetField.Value == 0 && foregroundLineOffsetField.Value == 0 && backgroundLineOffsetField.Value == 0)
                    {
                        var backgroundBuffer = new byte[pixelsPerLineField.Value * numberOfLineField.Value * backgroundFormat.GetColorDepth()];
                        outputBuffer = new byte[pixelsPerLineField.Value * numberOfLineField.Value * outputFormat.GetColorDepth()];

                        // we can optimize here and copy everything at once
                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                               (int)(pixelsPerLineField.Value * numberOfLineField.Value * foregroundFormat.GetColorDepth()),
                               converter: (foregroundBuffer, line) =>
                               {
                                   machine.SystemBus.ReadBytes(backgroundMemoryAddressRegister.Value, backgroundBuffer.Length, backgroundBuffer, 0);
                                   // per-pixel alpha blending
                                   blender.Blend(backgroundBuffer, foregroundBuffer, ref outputBuffer);
                                   return outputBuffer;
                               });
                    }
                    else
                    {
                        var backgroundLineBuffer = new byte[pixelsPerLineField.Value * backgroundFormat.GetColorDepth()];
                        var outputLineBuffer = new byte[pixelsPerLineField.Value * outputFormat.GetColorDepth()];

                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                               (int)(pixelsPerLineField.Value * foregroundFormat.GetColorDepth()),
                               (int)foregroundLineOffsetField.Value * foregroundFormat.GetColorDepth(),
                               (int)outputLineOffsetField.Value * outputFormat.GetColorDepth(),
                               (int)numberOfLineField.Value,
                               (foregroundBuffer, line) =>
                                {
                                    machine.SystemBus.ReadBytes(backgroundMemoryAddressRegister.Value + line * (backgroundLineOffsetField.Value + pixelsPerLineField.Value) * backgroundFormat.GetColorDepth(), backgroundLineBuffer.Length, backgroundLineBuffer, 0);
                                    blender.Blend(backgroundLineBuffer, foregroundBuffer, ref outputLineBuffer);
                                    return outputLineBuffer;
                                });
                    }
                break;
                case Mode.MemoryToMemoryWithPfc:
                    var converter = PixelManipulationTools.GetConverter(foregroundFormat, ENDIANESS, outputFormat, ENDIANESS);
                    foregroundFormat = foregroundColorModeField.Value.ToPixelFormat();

                    if(outputLineOffsetField.Value == 0 && foregroundLineOffsetField.Value == 0 && backgroundLineOffsetField.Value == 0)
                    {
                        outputBuffer = new byte[pixelsPerLineField.Value * numberOfLineField.Value * outputFormat.GetColorDepth()];
                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                                (int)(pixelsPerLineField.Value * numberOfLineField.Value * foregroundFormat.GetColorDepth()),
                                converter: (foregroundBuffer, line) =>
                                {
                                    converter.Convert(foregroundBuffer, ref outputBuffer);
                                    return outputBuffer;
                                });
                    }
                    else                    
                    {
                        var outputLineBuffer = new byte[pixelsPerLineField.Value * outputFormat.GetColorDepth()];
                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                                (int)pixelsPerLineField.Value * foregroundFormat.GetColorDepth(),
                                (int)foregroundLineOffsetField.Value * foregroundFormat.GetColorDepth(), 
                                (int)outputLineOffsetField.Value * outputFormat.GetColorDepth(),
                                (int)numberOfLineField.Value,
                                (foregroundBuffer, line) => 
                                {
                                    converter.Convert(foregroundBuffer, ref outputLineBuffer);
                                    return outputLineBuffer;
                                });
                    }
                break;
                case Mode.MemoryToMemory:
                    foregroundFormat = foregroundColorModeField.Value.ToPixelFormat();
                    if(outputLineOffsetField.Value == 0 && foregroundLineOffsetField.Value == 0)
                    {
                        // we can optimize here and copy everything at once
                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                                       (int)(pixelsPerLineField.Value * numberOfLineField.Value * foregroundFormat.GetColorDepth()));
                    }
                    else
                    {
                        // in this mode no graphical data transformation is performed
                        // color format is stored in foreground pfc control register
                        
                        DoCopy(foregroundMemoryAddressRegister.Value, outputMemoryAddressRegister.Value,
                                       (int)pixelsPerLineField.Value * foregroundFormat.GetColorDepth(),
                                       (int)foregroundLineOffsetField.Value * foregroundFormat.GetColorDepth(),
                                       (int)outputLineOffsetField.Value * foregroundFormat.GetColorDepth(),
                                       (int)numberOfLineField.Value);
                    }
                break;
            }

            startFlag.Value = false;
            IRQ.Set();
        }

        private void DoCopy(long sourceAddress, long destinationAddress, int chunkLength, int sourceOffset = 0, int destinationOffset = 0, int count = 1, Func<byte[], int, byte[]> converter = null)
        {
            var sourceBuffer = new byte[chunkLength];

            var currentSource = sourceAddress;
            var currentDestination = destinationAddress;

            for(int line = 0; line < count; line++)
            {
                machine.SystemBus.ReadBytes(currentSource, sourceBuffer.Length, sourceBuffer, 0);
                var destinationBuffer = converter == null ? sourceBuffer : converter(sourceBuffer, line);
                machine.SystemBus.WriteBytes(destinationBuffer, currentDestination, 0, destinationBuffer.Length);

                currentSource += sourceBuffer.Length + sourceOffset;
                currentDestination += destinationBuffer.Length + destinationOffset;
            }
        }

        private readonly Machine machine;
        private readonly DoubleWordRegister controlRegister;
        private readonly IFlagRegisterField startFlag;
        private readonly IEnumRegisterField<Mode> dma2dMode;
        private readonly DoubleWordRegister interruptFlagClearRegister;
        private readonly DoubleWordRegister numberOfLineRegister;
        private readonly IValueRegisterField numberOfLineField;
        private readonly IValueRegisterField pixelsPerLineField;
        private readonly DoubleWordRegister outputMemoryAddressRegister;
        private readonly DoubleWordRegister backgroundMemoryAddressRegister;
        private readonly DoubleWordRegister foregroundMemoryAddressRegister;
        private readonly DoubleWordRegister outputPfcControlRegister;
        private readonly IEnumRegisterField<Dma2DColorMode> outputColorModeField;
        private readonly DoubleWordRegister foregroundPfcControlRegister;
        private readonly IEnumRegisterField<Dma2DColorMode> foregroundColorModeField;
        private readonly DoubleWordRegister backgroundPfcControlRegister;
        private readonly IEnumRegisterField<Dma2DColorMode> backgroundColorModeField;
        private readonly DoubleWordRegister outputColorRegister;
        private readonly DoubleWordRegister outputOffsetRegister;
        private readonly IValueRegisterField outputLineOffsetField;
        private readonly DoubleWordRegister foregroundOffsetRegister;
        private readonly IValueRegisterField foregroundLineOffsetField;
        private readonly DoubleWordRegister backgroundOffsetRegister;
        private readonly IValueRegisterField backgroundLineOffsetField;
        private readonly DoubleWordRegisterCollection registers;

        private const ELFSharp.ELF.Endianess ENDIANESS = ELFSharp.ELF.Endianess.LittleEndian;

        private enum Mode
        {
            MemoryToMemory,
            MemoryToMemoryWithPfc,
            MemoryToMemoryWithBlending,
            RegisterToMemory
        }
    }
}
