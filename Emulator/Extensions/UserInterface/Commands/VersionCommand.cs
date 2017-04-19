//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using AntShell.Commands;
using Emul8.Core;

namespace Emul8.UserInterface.Commands
{
    public class VersionCommand : AutoLoadCommand
    {
        [Runnable]
        public void Run(ICommandInteraction writer)
        {
            writer.WriteLine(EmulationManager.Instance.VersionString);
        }

        public VersionCommand(Monitor monitor) : base(monitor, "version", "shows version information.")
        {
        }
    }
}

