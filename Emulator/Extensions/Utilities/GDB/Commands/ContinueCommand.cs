//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    internal class ContinueCommand : Command
    {
        public ContinueCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("c")]
        public PacketData Execute()
        {
            manager.Cpu.ExecutionMode = ExecutionMode.Continuous;
            return null;
        }
    }
}

