//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.UserInterface.Tokenizer;
using System.Collections.Generic;
using AntShell.Commands;
using System.Linq;

namespace Emul8.UserInterface.Commands
{
    public class StartCommand : Command
    {

        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Usage:");
            writer.WriteLine(String.Format("{0} - starts the current machine", Name));
            writer.WriteLine(String.Format("{0} @path - executes the script and starts the emulation", Name));
        }

        [Runnable]
        public void Run(ICommandInteraction writer)
        {
            var currentMachine = GetCurrentMachine();
            if(currentMachine == null)
            {
                writer.WriteError("Select active machine.");
                return;
            }
            writer.WriteLine("Starting emulation...");
            currentMachine.Start();        
        }

        [Runnable]
        public void Run(ICommandInteraction writer, PathToken path)
        {
            if(IncludeCommand.Run(writer, path))
            {
                EmulationManager.Instance.CurrentEmulation.StartAll();
            }
        }

        private readonly Func<Machine> GetCurrentMachine;
        private readonly IncludeFileCommand IncludeCommand;

        public StartCommand(Monitor monitor, Func<Machine> getCurrentMachine, IncludeFileCommand includeCommand) :  base(monitor, "start", "starts the emulation.", "s")
        {
            if(includeCommand == null)
            {
                throw new ArgumentException("includeCommand cannot be null.", "includeCommand");
            }
            GetCurrentMachine = getCurrentMachine;
            IncludeCommand = includeCommand;
        }
    }
}

