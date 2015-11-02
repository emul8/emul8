//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;

namespace Emul8.UserInterface.Commands
{
    public class VerboseCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine("Current value: " + verbose);
        }

        [Runnable]
        public void SetVerbosity(ICommandInteraction writer, BooleanToken verbosity)
        {
            verbose = verbosity.Value;
            setVerbosity(verbose);
        }

        public VerboseCommand(Monitor monitor, Action<bool> setVerbosity) : base(monitor, "verboseMode", "controls the verbosity of the Monitor.")
        {
            verbose = false;
            this.setVerbosity = setVerbosity;
        }

        private readonly Action<bool> setVerbosity;
        private bool verbose;
    }
}

