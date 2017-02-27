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
using System.Linq;

namespace Emul8.UserInterface.Commands
{
    public class HelpCommand : Command
    {
        [Runnable]
        public void GeneralHelp(ICommandInteraction writer)
        {
            writer.WriteLine("Available commands:");
            writer.WriteLine(string.Format("{0,-18}| {1}", "Name", "Description"));
            writer.WriteLine("================================================================================");
            foreach(var item in GetCommands().OrderBy(x=>x.Name))
            {
                writer.WriteLine(string.Format("{0,-18}: {1}", item.Name, item.Description));    
            }
            writer.WriteLine();
            writer.WriteLine("You can also provide a device name to access its methods.");
            writer.WriteLine("Use <TAB> for auto-completion.");
        }

        [Runnable]
        public void CommandHelp(ICommandInteraction writer, LiteralToken commandName)
        {
            if(!GetCommands().Any(x => x.Name == commandName.Value))
            {
                writer.WriteError(String.Format("No such command: {0}.", commandName.Value));
                return;
            }
            var command = GetCommands().First(x => x.Name == commandName.Value);
            command.PrintHelp(writer);
        }

        private readonly Func<IEnumerable<ICommandDescription>> GetCommands;

        public HelpCommand(Monitor monitor, Func<IEnumerable<ICommandDescription>> getCommands) : base(monitor, "help", "prints this help message or info about specified command.", "?", "h")
        {
            GetCommands = getCommands;
        }
    }
}

