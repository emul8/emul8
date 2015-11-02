//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using System.Collections.Generic;
using Emul8.UserInterface.Tokenizer;
using System.Linq;

namespace Emul8.UserInterface.Commands
{
    public class NumbersModeCommand : AutoLoadCommand
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();

            writer.WriteLine(string.Format("Current mode: {0}", monitor.CurrentNumberFormat));
            writer.WriteLine();
            writer.WriteLine("Options:");
            foreach(var item in typeof(Monitor.NumberModes).GetEnumNames())
            {
                writer.WriteLine(item);
            }
        }

        [Runnable]
        public void Run(ICommandInteraction writer,
                        [Values("Both", "Decimal", "Hexadecimal")] LiteralToken format)
        {
            monitor.CurrentNumberFormat = (Monitor.NumberModes)Enum.Parse(typeof(Monitor.NumberModes), format.Value);
        }

        public NumbersModeCommand(Monitor monitor):base(monitor, "numbersMode", "sets the way numbers are displayed.")
        {

        }
    }
}

