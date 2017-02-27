//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using AntShell.Commands;

namespace Emul8.UserInterface.Commands
{
    public class PauseCommand : AutoLoadCommand
    {
        [Runnable]
        public void Halt(ICommandInteraction writer)
        {
            writer.WriteLine("Pausing emulation...");
            EmulationManager.Instance.CurrentEmulation.PauseAll();
        }

        public PauseCommand(Monitor monitor) :  base(monitor, "pause", "pauses the emulation.", "p")
        {
        }
    }
}

