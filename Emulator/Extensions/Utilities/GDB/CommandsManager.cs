//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB
{
    internal class CommandsManager
    {
        public CommandsManager(TranslationCPU cpu)
        {
            availableCommands = new HashSet<CommandDescriptor>();
            activeCommands = new HashSet<Command>();

            Machine machine;
            EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(cpu, out machine);
            Cpu = cpu;
            Machine = machine;
        }

        public void Register(Type t)
        {
            if(t == typeof(Command) || !typeof(Command).IsAssignableFrom(t))
            {
                return;
            }

            var interestingMethods = Command.GetExecutingMethods(t);
            if(interestingMethods.Length == 0)
            {
                Logger.Log(LogLevel.Warning, string.Format("No executing methods found in type {0}", t.Name));
                return;
            }

            foreach(var interestingMethod in interestingMethods)
            {
                availableCommands.Add(new CommandDescriptor(interestingMethod));
            }
        }

        public bool TryGetCommand(Packet packet, out Command command)
        {
            var commandDescriptor = availableCommands.SingleOrDefault(x => packet.Data.DataAsString.StartsWith(x.Mnemonic, StringComparison.Ordinal));
            if(commandDescriptor == null)
            {
                command = null;
                return false;
            }

            command = GetOrCreateCommand(commandDescriptor.Method.DeclaringType);
            return true;
        }

        public Machine Machine { get; private set; }
        public TranslationCPU Cpu { get; private set; }

        private Command GetOrCreateCommand(Type t)
        {
            var result = activeCommands.SingleOrDefault(x => x.GetType() == t);
            if(result == null)
            {
                result = (Command)Activator.CreateInstance(t, new[] { this });
                activeCommands.Add(result);
            }

            return result;
        }

        private readonly HashSet<CommandDescriptor> availableCommands;
        private readonly HashSet<Command> activeCommands;

        private class CommandDescriptor
        {
            public CommandDescriptor(MethodInfo method)
            {
                Method = method;
                Mnemonic = method.GetCustomAttribute<ExecuteAttribute>().Mnemonic;
            }

            public string Mnemonic { get; private set; }
            public MethodInfo Method { get; private set; }
        }
    }
}

