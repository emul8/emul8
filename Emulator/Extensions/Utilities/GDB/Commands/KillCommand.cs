//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Utilities.GDB.Commands
{
    internal class KillCommand : Command
    {
        public KillCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("k")]
        public PacketData Execute()
        {
            return PacketData.Success;
        }
    }
}

