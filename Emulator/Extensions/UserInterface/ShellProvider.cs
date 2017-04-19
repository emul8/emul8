//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell;
using System.IO;
using Emul8.UserInterface.Commands;
using Emul8.Utilities;
using AntShell.Terminal;
using System.Linq;
using Emul8.Core;

namespace Emul8.UserInterface
{
    public static class ShellProvider
    {
        public static Shell GenerateShell(IOProvider io, Monitor monitor, bool forceVCursor = false, bool clearScreen = true)
        {
            var settings = new ShellSettings { 
                NormalPrompt = new Prompt("(monitor) ", ConsoleColor.DarkRed),
                Banner = Enumerable.Repeat(Environment.NewLine, NumberOfDummyLines).Aggregate(String.Empty, (x, y) => x + y) + EmulationManager.Instance.VersionString,
                UseBuiltinQuit = false,
                UseBuiltinHelp = false,
                UseBuiltinSave = false,
                ForceVirtualCursor = forceVCursor,
                ClearScreen = clearScreen,
                HistorySavePath = ConfigurationManager.Instance.Get("general", "history-path", Path.Combine(Misc.GetUserDirectory(), "history"))
            };

            var shell = new Shell(io, monitor, settings);

            var startupCommand = Environment.GetEnvironmentVariable(Monitor.StartupCommandEnv);
            if(!string.IsNullOrEmpty(startupCommand) && shell != null)
            {
                shell.StartupCommand = startupCommand;
            }

            return shell;
        }

        public static int NumberOfDummyLines;
    }
}

