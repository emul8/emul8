//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.UserInterface.Commands;
using Emul8.UserInterface;
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;

namespace Emul8.Plugins.SampleCommandPlugin
{
    public sealed class HelloCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Usage:");
            writer.WriteLine(String.Format("{0} \"name\"", Name));
        }

        [Runnable]
        public void Run(ICommandInteraction writer, StringToken name)
        {
            writer.WriteLine(String.Format("Hello, {0}!", name.Value));
        }

        public HelloCommand(Monitor monitor) : base(monitor, "hello", "Greets a user.")
        {
        }
    }
}
