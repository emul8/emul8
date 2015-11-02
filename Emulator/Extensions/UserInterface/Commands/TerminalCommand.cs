//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.UserInterface.Tokenizer;
using AntShell.Commands;
using Emul8.Backends.Terminals;

namespace Emul8.UserInterface.Commands
{
    public class TerminalCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Available terminals:");
            foreach(var key in console.Keys)
            {
                writer.WriteLine(string.Format("\t{0}", key));
            }
        }

        [Runnable]
        public void Run(ICommandInteraction writer, StringToken name)
        {
            console.SetActiveTerminal(name.Value);
        }

        private ConsoleTerminal console;

        public TerminalCommand(Monitor monitor, ConsoleTerminal console) : base(monitor, "terminal", "manipulates the active terminal.", "term")
        {
            this.console = console;
        }
    }
}

