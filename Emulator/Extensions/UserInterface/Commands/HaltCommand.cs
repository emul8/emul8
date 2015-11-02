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

namespace Emul8.UserInterface.Commands
{
    public class HaltCommand : Command
    {
        [Runnable]
        public void Halt(ICommandInteraction writer)
        {
            var currentMachine = GetCurrentMachine();
            if(currentMachine == null)
            {
                writer.WriteError("Select active machine.");
                return;
            }
            writer.WriteLine("Pausing emulation...");
            currentMachine.Pause();        
        }

        private Func<Machine> GetCurrentMachine;

        public HaltCommand(Monitor monitor, Func<Machine> getCurrentMachine) :  base(monitor, "halt", "stops the emulation.", "h")
        {
            GetCurrentMachine = getCurrentMachine;
        }
    }
}

