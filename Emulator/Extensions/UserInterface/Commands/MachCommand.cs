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
using Emul8.Utilities;

namespace Emul8.UserInterface.Commands
{
    public class MachCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            var currentMachine = GetCurrentMachine();
            if(currentMachine == null)
            {
                writer.WriteError("No machine selected.");
            }
            else
            {
                writer.WriteLine(string.Format("Current machine: {0}", EmulationManager.Instance.CurrentEmulation[currentMachine]));
            }
            if(EmulationManager.Instance.CurrentEmulation.MachinesCount > 0)
            {
                writer.WriteLine("Available machines:");
                var emu = EmulationManager.Instance.CurrentEmulation;
                var longestNameLength = emu.Machines.Max(m => emu[m].Length);
                var i = 0;
                foreach(var machine in emu.Machines)
                {
                    writer.WriteLine(string.Format("\t{2}: {0,-" + longestNameLength + "} {1}", emu[machine], machine.Platform != null ? string.Format("[{0}]", machine.Platform.Name) : string.Empty, i++));
                }
            }
            writer.WriteLine();
            writer.WriteLine("You can use the following commands:");
            writer.WriteLine("'mach set [\"name\"|number]'\tto enable the given machine");
            writer.WriteLine("'mach add \"name\"'\tto create a new machine with the specified name");
            writer.WriteLine("'mach rem \"name\"'\tto remove a machine");
            writer.WriteLine("'mach create'\tto create a new machine with generic name and switch to it");
            writer.WriteLine("'mach clear'\tto clear the current selection");
        }

        [Runnable]
        public void Run(ICommandInteraction writer, [Values("set")] LiteralToken action, NumericToken number)
        {
            var machines = EmulationManager.Instance.CurrentEmulation.Machines.ToArray();
            if(machines.Length > number.Value && number.Value >= 0)
            {
                SetCurrentMachine(machines[number.Value]);
            }
            else
            {
                writer.WriteError("Wrong machine number. Type {0} to show a list of available machines.".FormatWith(Name));
            }

        }

        [Runnable]
        public void Run(ICommandInteraction writer, [Values("set", "add", "rem")] LiteralToken action, StringToken name)
        {
            switch(action.Value)
            {
            case "add":       
                var machine = new Machine();
                EmulationManager.Instance.CurrentEmulation.AddMachine(machine, name.Value);
                if(GetCurrentMachine() == null)
                {
                    SetCurrentMachine(machine);
                }
                break;
            case "set":
                try
                {
                    SetCurrentMachine(EmulationManager.Instance.CurrentEmulation[name.Value]);
                }
                catch(KeyNotFoundException)
                {
                    writer.WriteError(string.Format("Machine {0} not found.", name.Value));
                }
                break;
            case "rem":
                var machineToRemove = EmulationManager.Instance.CurrentEmulation[name.Value];
                EmulationManager.Instance.CurrentEmulation.RemoveMachine(name.Value);
                if(GetCurrentMachine() == machineToRemove)
                {
                    SetCurrentMachine(null);
                }
                break;
            }
        }

        [Runnable]
        public void Run(ICommandInteraction writer, [Values("create", "clear")] LiteralToken action)
        {
            switch(action.Value)
            {
            case "clear":
                // TODO: is that all that should be done on clear?
                SetCurrentMachine(null);
                break;
            case "create":
                var machine = new Machine();
                EmulationManager.Instance.CurrentEmulation.AddMachine(machine);
                SetCurrentMachine(machine);
                break;
            }
        }

        private Func<Machine> GetCurrentMachine;
        private Action<Machine> SetCurrentMachine;

        public MachCommand(Monitor monitor, Func<Machine> getCurrentMachine, Action<Machine> setCurrentMachine) 
            : base(monitor, "mach", "list and manipulate machines available in the environment.")
        {
            GetCurrentMachine = getCurrentMachine;
            SetCurrentMachine = setCurrentMachine;
        }
    }
}

