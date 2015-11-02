//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using Antmicro.Migrant;
using AntShell.Terminal;
using Emul8.Exceptions;
using System;

namespace Emul8.Backends.Terminals
{
    [Transient]
	public class ConsoleTerminal
	{
        public ConsoleTerminal(IIOSource world)
        {
            multiplexer = new TerminalMultiplexer(world);
        }

        public void SetActiveTerminal(string name)
        {
            try 
            {
                multiplexer.ChangeTerminalTo(name);
            } 
            catch (Exception)
            {
                throw new RecoverableException(string.Format("Terminal '{0}' not found.", name));
            }
        }

        public void AttachTerminal(string name, BasicTerminalEmulator terminal)
        {
            multiplexer.AddTerminal(name, terminal);
        }

        public void AttachTerminal(string name, DetachableIO io)
        {
            AttachTerminal(name, new BasicTerminalEmulator(io));
        }

        public void DetachTerminal(string name)
        {
            multiplexer.RemoveTerminal(name);
        }

        public IEnumerable<string> Keys { get { return multiplexer.AvailableTerminals(); } } 

        private readonly TerminalMultiplexer multiplexer;
	}
}
