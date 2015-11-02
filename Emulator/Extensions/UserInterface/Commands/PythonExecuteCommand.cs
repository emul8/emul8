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
    public class PythonExecuteCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Provide a command or variable to execute.");
        }

        [Runnable]
        public void Run(ICommandInteraction writer, VariableToken variable)
        {
            var value = GetVariable(variable);
            Run(writer, (StringToken)value);
        }

        [Runnable]
        public void Run(ICommandInteraction writer, StringToken command)
        {
            Execute(command.Value, writer);
        }

        private readonly Func<VariableToken, Token> GetVariable;
        private readonly Action<string, ICommandInteraction> Execute;

        public PythonExecuteCommand(Monitor monitor, Func<VariableToken, Token> getVariable, Action<String, ICommandInteraction> execute) 
            :base(monitor, "python", "executes the provided python command.", "py") 
        {
            GetVariable = getVariable;
            Execute = execute;
        }
    }
}

