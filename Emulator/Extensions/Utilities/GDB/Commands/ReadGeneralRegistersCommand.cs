//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("g")]
    internal class ReadGeneralRegistersCommand : Command
    {
        public ReadGeneralRegistersCommand(IControllableCPU cpu)
        {
            this.cpu = cpu;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var registers = new StringBuilder();
            foreach(var i in cpu.GetRegisters())
            {
                var value = cpu.GetRegisterUnsafe(i);
                foreach(var b in BitConverter.GetBytes(value))
                {
                    registers.AppendFormat("{0:x2}", b);
                }
            }

            return new PacketData(registers.ToString());
        }

        private readonly IControllableCPU cpu;
    }
}

