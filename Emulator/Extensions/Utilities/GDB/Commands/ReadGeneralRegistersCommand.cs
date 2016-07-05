//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;

namespace Emul8.Utilities.GDB.Commands
{
    internal class ReadGeneralRegistersCommand : Command
    {
        public ReadGeneralRegistersCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("g")]
        public PacketData Execute()
        {
            var registers = new StringBuilder();
            foreach(var i in manager.Cpu.GetRegisters())
            {
                var value = manager.Cpu.GetRegisterUnsafe(i);
                foreach(var b in BitConverter.GetBytes(value))
                {
                    registers.AppendFormat("{0:x2}", b);
                }
            }

            return new PacketData(registers.ToString());
        }
    }
}

