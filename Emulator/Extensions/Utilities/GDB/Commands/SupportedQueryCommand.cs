//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Utilities.GDB.Commands
{
    internal class SupportedQueryCommand : Command
    {
        public SupportedQueryCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("qSupported")]
        public PacketData Execute()
        {
            return new PacketData(string.Format("PacketSize={0:x4}", 4096));
        }
    }
}

