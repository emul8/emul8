//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Emul8.Utilities.GDB
{
    internal class CommandsManager
    {
        public CommandsManager()
        {
            commands = new Dictionary<string, Command>();
        }

        public void Register(Command cmd)
        {
            var mnemonicAttribute = cmd.GetType().GetCustomAttribute<MnemonicAttribute>();
            commands.Add(mnemonicAttribute.Mnemonic, cmd);
        }

        public bool TryGetCommand(string packet, out Command command)
        {
            command = commands.FirstOrDefault(x => packet.StartsWith(x.Key, System.StringComparison.Ordinal)).Value;
            return command != null;
        }

        private readonly Dictionary<string, Command> commands;
    }
}

