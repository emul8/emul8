//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Utilities.GDB
{
    internal class CommandsManager
    {
        public CommandsManager()
        {
            commands = new List<Command>();
        }

        public void Register(Command cmd)
        {
            commands.Add(cmd);
        }

        public bool TryGetCommand(string packet, out Command command)
        {
            command = commands.FirstOrDefault(x => packet.StartsWith(x.Mnemonic, System.StringComparison.Ordinal));
            return command != null;
        }

        private readonly List<Command> commands;
    }
}

