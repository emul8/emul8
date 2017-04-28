//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    internal class SingleStepCommand : Command
    {
        public SingleStepCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("s")]
        public PacketData Execute()
        {
            manager.Cpu.ExecutionMode = ExecutionMode.SingleStep;
            manager.Cpu.Step(wait: false);
            return null;
        }
    }
}

