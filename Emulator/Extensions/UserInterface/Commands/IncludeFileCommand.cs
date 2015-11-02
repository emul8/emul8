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
using System.IO;
using Emul8.Core;

namespace Emul8.UserInterface.Commands
{
    public class IncludeFileCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);

            writer.WriteLine("\nTo load a script you have to provide an existing file name.");
            writer.WriteLine();
            writer.WriteLine("Supported file formats:");
            writer.WriteLine("*.cs  - plugin file");
            writer.WriteLine("*.py  - python script");
            writer.WriteLine("other - monitor script");
        }

        [Runnable]
        public bool Run(ICommandInteraction writer, PathToken path)
        {
            if(!File.Exists(path.Value))
            {
                writer.WriteError(String.Format("No such file {0}.", path.Value));
                return false;
            }

            using(var progress = EmulationManager.Instance.ProgressMonitor.Start("Including script: " + path.Value))
            {
                bool result = false;
                switch(Path.GetExtension(path.Value))
                {
                case ".py":
                    result = PythonExecutor(path.Value, writer);
                    break;
                case ".cs":
                    result = CsharpExecutor(path.Value, writer);
                    break;
                default:
                    result = ScriptExecutor(path.Value);
                    break;
                }

                return result;
            }
        }

        private readonly Func<string,bool> ScriptExecutor;
        private readonly Func<string, ICommandInteraction, bool> CsharpExecutor;
        private readonly Func<string, ICommandInteraction, bool> PythonExecutor;

        public IncludeFileCommand(Monitor monitor, Func<string, ICommandInteraction, bool> pythonExecutor, Func<string, bool> scriptExecutor, Func<string, ICommandInteraction, bool> csharpExecutor) : base(monitor, "include", "loads a monitor script, python code or a plugin class.", "i")
        {
            this.CsharpExecutor = csharpExecutor;
            this.PythonExecutor = pythonExecutor;
            this.ScriptExecutor = scriptExecutor;
        }
    }
}

