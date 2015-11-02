//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using AntShell.Commands;
using System.Text;
using System.Linq;
using Emul8.Utilities;

namespace Emul8.UserInterface.Commands
{

    public abstract class AutoLoadCommand : Command, IAutoLoadType
    {
        protected AutoLoadCommand(Monitor monitor, string name, string description, params string[] alternativeNames) : base(monitor, name, description, alternativeNames)
        {
        }
    }

    public abstract class Command : IInterestingType, ICommandDescription
    {
        protected readonly Monitor monitor;

		protected Command(Monitor monitor, string name, string description, params string[] alternativeNames)
		{
			this.monitor = monitor;
			Description = description;
			Name = name;
			AlternativeNames = alternativeNames;
		}

        public string[] AlternativeNames {get; private set;}
        public string Name { get; private set; }
        public string Description{ get; private set; }

        public virtual void PrintHelp(ICommandInteraction writer)
        {
            writer.WriteLine(this.GetHelp());
        }
    }

    public class CommandComparer : IEqualityComparer<Command>
    {
        public bool Equals(Command x, Command y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Command obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}

