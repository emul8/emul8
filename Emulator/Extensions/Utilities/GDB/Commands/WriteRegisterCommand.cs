//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;

namespace Emul8.Utilities.GDB.Commands
{
    internal class WriteRegisterCommand : Command
    {
        public WriteRegisterCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("P")]
        public PacketData Execute(
            [Argument(Separator = '=')]int registerNumber, 
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexBytesString)]uint value)
        {
            if(!manager.Cpu.GetRegisters().Contains(registerNumber))
            {
                return PacketData.ErrorReply(0);
            }

            manager.Cpu.SetRegisterUnsafe(registerNumber, value);
            return PacketData.Success;
        }
    }
}

