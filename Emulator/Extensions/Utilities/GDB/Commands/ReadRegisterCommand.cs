//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using System.Text;

namespace Emul8.Utilities.GDB.Commands
{
    internal class ReadRegisterCommand : Command
    {
        public ReadRegisterCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("p")]
        public PacketData Execute(
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]int registerNumber)
        {
            var content = new StringBuilder();
            var value = manager.Cpu.GetRegisters().Contains(registerNumber) ? manager.Cpu.GetRegisterUnsafe(registerNumber) : 0;
            foreach(var b in BitConverter.GetBytes(value))
            {
                content.AppendFormat("{0:x2}", b);
            }
            return new PacketData(content.ToString());
        }
    }
}

