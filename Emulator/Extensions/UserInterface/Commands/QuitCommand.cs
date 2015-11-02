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
using Emul8.Logging;
using Emul8.Core;

namespace Emul8.UserInterface.Commands
{
    public class QuitCommand : Command
    {
        [Runnable]
        public void Run(ICommandInteraction writer)
        {
            SetCurrentMachine(null);
            var quit = Quitted;
            if(quit != null)
            {
                var exQuit = quit();
                if(exQuit != null)
                {
                    exQuit();
                }
            }
            writer.QuitEnvironment = true;
        }

        private Action<Machine> SetCurrentMachine;
        private event Func<Action> Quitted;

        public QuitCommand(Monitor monitor, Action<Machine> setCurrentMachine, Func<Action> quitted) : base(monitor, "quit", "quits the emulator.", "q")
        {
            SetCurrentMachine = setCurrentMachine;
            Quitted = quitted;
        }
    }
}

