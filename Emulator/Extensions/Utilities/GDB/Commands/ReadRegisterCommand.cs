//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.Peripherals.CPU;
using ELFSharp.ELF;

namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("p")]
    internal class ReadRegisterCommand : Command
    {
        public ReadRegisterCommand(IControllableCPU cpu)
        {
            this.cpu = cpu;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var splittedArguments = GetCommandArguments(packet.Data);
            if(splittedArguments.Length != 1)
            {
                throw new ArgumentException("Expected one argument");
            }

            int registerNumber;
            if(!int.TryParse(splittedArguments[0], System.Globalization.NumberStyles.HexNumber, null, out registerNumber))
            {
                throw new ArgumentException("Could not parse register number");
            }

            var value = cpu.GetRegisters().Contains(registerNumber) ? cpu.GetRegisterUnsafe(registerNumber) : 0;
            if(cpu.Endianness == Endianess.LittleEndian)
            {
                value = Helpers.SwapBytes(value);
            }

            return new PacketData(string.Format("{0:x8}", value));
        }

        private readonly IControllableCPU cpu;
    }
}

