//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Text;

namespace Emul8.Utilities.GDB.Commands
{
    internal class ReadMemoryCommand : Command
    {
        public ReadMemoryCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("m")]
        public PacketData Execute(
            [Argument(Separator = ',', Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]uint address,
            [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]int length)
        {
            var content = new StringBuilder();
            foreach(var b in manager.Machine.SystemBus.ReadBytes(address, length))
            {
                content.AppendFormat("{0:x2}", b);
            }

            return new PacketData(content.ToString());
        }
    }
}

