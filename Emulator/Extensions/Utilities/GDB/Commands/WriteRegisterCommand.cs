//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;
using System;

namespace Emul8.Utilities.GDB.Commands
{
    internal class WriteRegisterCommand : Command
    {
        public WriteRegisterCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("P")]
        public PacketData Execute(
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber, Separator = '=')]int registerNumber,
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexBytesString)]byte[] value)
        {
            if(!manager.Cpu.GetRegisters().Contains(registerNumber))
            {
                return PacketData.ErrorReply(0);
            }

            manager.Cpu.SetRegisterUnsafe(registerNumber, BitConverter.ToUInt32(value, 0));
            return PacketData.Success;
        }
    }
}

