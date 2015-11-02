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

namespace Emul8.UserInterface.Commands
{
    public class MonitorPathCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            PrintCurrentPath(writer);
            writer.WriteLine(string.Format("Default 'PATH' value is: {0}", monitorPath.DefaultPath));
            writer.WriteLine();
            writer.WriteLine("You can use following commands:");
            writer.WriteLine(String.Format("'{0} set @path'\tto set 'PATH' to the given value", Name));
            writer.WriteLine(String.Format("'{0} add @path'\tto append the given value to 'PATH'", Name));
            writer.WriteLine(String.Format("'{0} reset'\t\tto reset 'PATH' to it's default value", Name));
        }
        private void PrintCurrentPath(ICommandInteraction writer)
        {
            writer.WriteLine(string.Format("Current 'PATH' value is: {0}", monitorPath.Path));
        }

        [Runnable]
        public void SetOrAdd(ICommandInteraction writer, [Values( "set", "add")] LiteralToken action, PathToken path)
        {
            switch(action.Value)
            {
            case "set":
                monitorPath.Path = path.Value;
                break;
            case "add":
                monitorPath.Append(path.Value);
                break;
            }
            PrintCurrentPath(writer);
        }

        [Runnable]
        public void Reset(ICommandInteraction writer, [Values( "reset")] LiteralToken action)
        {
            monitorPath.Path = monitorPath.DefaultPath;
            PrintCurrentPath(writer);
        }

        MonitorPath monitorPath;
        public MonitorPathCommand(Monitor monitor, MonitorPath path) : base(monitor, "path", "allows modification of internal 'PATH' variable.")
        {
            monitorPath = path;
        }
    }
}

