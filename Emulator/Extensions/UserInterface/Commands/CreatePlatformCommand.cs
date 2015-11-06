//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;
using Emul8.Core;
using System;
using System.Linq;

namespace Emul8.UserInterface.Commands
{
    public class CreatePlatformCommand : Command, ISuggestionProvider
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine("\nOptions:");
            writer.WriteLine("===========================");
            foreach(var item in PlatformsProvider.GetAvailablePlatforms().OrderBy(x=>x.Name))
            {
                writer.WriteLine(item.Name);
            }
        }

        #region ISuggestionProvider implementation

        public string[] ProvideSuggestions(string prefix)
        {
            return PlatformsProvider.GetAvailablePlatforms().Where(p => p.Name.StartsWith(prefix)).Select(p => p.Name).ToArray();
        }

        #endregion

        [Runnable]
        public void Run(ICommandInteraction writer, LiteralToken type)
        {
            Execute(writer, type.Value, null);
        }

        [Runnable]
        public void Run(ICommandInteraction writer, LiteralToken type, StringToken name)
        {
            Execute(writer, type.Value, name.Value);
        }

        private void Execute(ICommandInteraction writer, string type, string name)
        {
            var platform = PlatformsProvider.GetPlatformByName(type);
            if (platform == null)
            {
                writer.WriteError("Invalid platform type: " + type);
                return;
            }

            var mach = new Machine() { Platform = platform };
            EmulationManager.Instance.CurrentEmulation.AddMachine(mach, name);
            changeCurrentMachine(mach);
            monitor.TryExecuteScript(platform.ScriptPath);
        }

        public CreatePlatformCommand(Monitor monitor, Action<Machine> changeCurrentMachine) : base(monitor, "createPlatform", "creates a platform.", "c")
        {
            this.changeCurrentMachine = changeCurrentMachine;
        }

        private readonly Action<Machine> changeCurrentMachine;
    }
}

