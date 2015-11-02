//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.UserInterface.Tokenizer;
using AntShell.Commands;
using Emul8.Logging;
using Emul8.Core;

namespace Emul8.UserInterface.Commands
{
    public class LoggerFileCommand : AutoLoadCommand
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteError("\nYou must specify the filename (full path or relative) for output file.");
        }

        [Runnable]
        public void Run(PathToken path)
        {
            Logger.AddBackend(new FileBackend(path.Value), "file", true);
        }

        public LoggerFileCommand(Monitor monitor) : base(monitor, "logFile", "sets the output file for logger.", "logF")
        {
        }
    }
}

