//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Utilities.GDB.Commands
{
    internal class ReportHaltReasonCommand : Command
    {
        public ReportHaltReasonCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("?")]
        public PacketData Execute()
        {
            return PacketData.StopReply(0);
        }
    }
}

