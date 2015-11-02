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
using Emul8.Core;
using System.Linq;
using Emul8.Core.Structure;
using Emul8.Utilities;

namespace Emul8.UserInterface.Commands
{
    public class PeripheralsCommand : Command
    {
        [Runnable]
        public void Run(ICommandInteraction writer)
        {
            var currentMachine = GetCurrentMachine();
            if( currentMachine == null)
            {
                writer.WriteError("Select active machine.");
                return;
            }
            writer.WriteLine("Available peripherals:");
            var peripheralEntries = currentMachine.GetPeripheralsWithAllRegistrationPoints();
            foreach(var entry in peripheralEntries)
            {
                var isInMachine = entry.Key.Parent == null;
                var description = (isInMachine || entry.Key.RegistrationPoint is ITheOnlyPossibleRegistrationPoint) ? "{0} ({1}) in {2}\n\r" : "{0} ({1}) in {2} at ";
                var parentName = !isInMachine ? peripheralEntries.First(x => x.Key.Peripheral == entry.Key.Parent).Key.Name
                    : EmulationManager.Instance.CurrentEmulation[currentMachine];
                var peripheralName = entry.Key.Name ?? "<unnamed>";
                var nameAndParent = string.Format(description, peripheralName, entry.Key.Type.Name, parentName);
                writer.Write(nameAndParent);
                if(!(entry.Key.RegistrationPoint is ITheOnlyPossibleRegistrationPoint || isInMachine))
                {
                    writer.WriteLine(entry.Key.RegistrationPoint.PrettyString);
                    if(entry.Value.Count() > 1)
                    {
                        foreach(var otherEntry in entry.Value.Skip(1))
                        {
                            writer.WriteLine(String.Format("{1}and at {0}", otherEntry.PrettyString, " ".PadLeft(nameAndParent.Length - "and at".Length-1 )));
                        }
                    }
                }
            }
        }

        Func<Machine> GetCurrentMachine;

        public PeripheralsCommand(Monitor monitor, Func<Machine> getCurrentMachine) : base(monitor, "peripherals", "prints list of registered and named peripherals.", "peri")
        {
            GetCurrentMachine = getCurrentMachine;
        }
    }
}

