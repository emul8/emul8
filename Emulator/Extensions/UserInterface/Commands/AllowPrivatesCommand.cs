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
using System.Reflection;
using AntShell.Commands;

namespace Emul8.UserInterface.Commands
{
    public class AllowPrivatesCommand : AutoLoadCommand
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            var allowed = (monitor.CurrentBindingFlags & BindingFlags.NonPublic) > 0;
            writer.WriteLine();
            writer.WriteLine(allowed ? "Private fields are available":"Private fields are not available");
            return;
        }

        [Runnable]
        public void RunnableAttribute(ICommandInteraction writer, BooleanToken allow)
        {
            if(allow.Value)
            {
                monitor.CurrentBindingFlags |= BindingFlags.NonPublic;
            }
            else
            {
                monitor.CurrentBindingFlags &= ~BindingFlags.NonPublic;
            }
        }

        public AllowPrivatesCommand(Monitor monitor):base(monitor, "allowPrivates","allow private fields and properties manipulation.", "privs")
        {
        }
    }
}

