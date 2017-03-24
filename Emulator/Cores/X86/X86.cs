//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Endianess = ELFSharp.ELF.Endianess;
using Emul8.Core;
using Emul8.Utilities.Binding;
using Emul8.Peripherals.IRQControllers;

namespace Emul8.Peripherals.CPU
{
    [GPIO(NumberOfInputs = 1)]
    public partial class X86 : TranslationCPU
    {
        const Endianess endianness = Endianess.LittleEndian;

        public X86(string cpuType, Machine machine, LAPIC lapic): base(cpuType, machine, endianness)
        {
            this.lapic = lapic;
        }

        public override string Architecture { get { return "i386"; } }

        protected override Interrupt DecodeInterrupt(int number)
        {
            if(number == 0)
            {
                return Interrupt.Hard;
            }
            throw InvalidInterruptNumberException;
        }

        [Export]
        private uint ReadByteFromPort(uint address)
        {
            return ReadByteFromBus(IoPortBaseAddress + address);
        }

        [Export]
        private uint ReadWordFromPort(uint address)
        {
            return ReadWordFromBus(IoPortBaseAddress + address);
        }

        [Export]
        private uint ReadDoubleWordFromPort(uint address)
        {
            return ReadDoubleWordFromBus(IoPortBaseAddress + address);
        }

        [Export]
        private void WriteByteToPort(uint address, uint value)
        {
            WriteByteToBus(IoPortBaseAddress + address, value);

        }

        [Export]
        private void WriteWordToPort(uint address, uint value)
        {
            WriteWordToBus(IoPortBaseAddress + address, value);
        }

        [Export]
        private void WriteDoubleWordToPort(uint address, uint value)
        {
            WriteDoubleWordToBus(IoPortBaseAddress + address, value);
        }

        [Export]
        private int GetPendingInterrupt()
        {
            return lapic.GetPendingInterrupt();
        }

        [Export]
        private ulong GetInstructionCount()
        {
            return (ulong)this.ExecutedInstructions;
        }

        private readonly LAPIC lapic;
        private const uint IoPortBaseAddress = 0xE0000000;
    }
}

