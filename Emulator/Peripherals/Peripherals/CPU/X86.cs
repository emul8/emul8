//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using Emul8.Utilities.Binding;

namespace Emul8.Peripherals.CPU
{
    public partial class X86 : TranslationCPU 
    {
        const EndiannessEnum endianness = EndiannessEnum.LittleEndian;

        public X86(string cpuType, Machine machine): base(cpuType, machine, endianness) { }

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

        private const uint IoPortBaseAddress = 0xE0000000;
    }
}

