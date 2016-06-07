//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using ELFSharp.ELF;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("P")]
    internal class WriteRegisterCommand : Command
    {
        public WriteRegisterCommand(IControllableCPU cpu)
        {
            this.cpu = cpu;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var splittedArguments = GetCommandArguments(packet.Data, Separators, 2);
            if(splittedArguments.Length != 2)
            {
                throw new ArgumentException("Expected two arguments");
            }

            int registerNumber;
            if(!int.TryParse(splittedArguments[0], System.Globalization.NumberStyles.HexNumber, null, out registerNumber))
            {
                throw new ArgumentException("Could not parse register number");
            }

            uint value;
            if(!uint.TryParse(splittedArguments[1], System.Globalization.NumberStyles.HexNumber, null, out value))
            {
                throw new ArgumentException("Could not parse value");
            }

            if(cpu.Endianness == Endianess.LittleEndian)
            {
                value = Helpers.SwapBytes(value);
            }

            cpu.SetRegisterUnsafe(registerNumber, value);

            return PacketData.Success;
        }

        private readonly IControllableCPU cpu;
        private static readonly char[] Separators = { '=' };
    }
}

