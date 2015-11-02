//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;
using System;
using Emul8.Utilities;
using System.Collections.Generic;
using Emul8.Exceptions;

namespace Emul8.UserInterface.Commands
{
    public class ExecuteCommand : Command
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Provide a command or {0} to execute.".FormatWith(noun));
            writer.WriteLine();
            writer.WriteLine("Available {0}s:".FormatWith(noun));
            foreach(var variable in GetVariables())
            {
                writer.WriteLine("\t{0}".FormatWith(variable));
            }
        }

        [Runnable]
        public virtual void Run(ICommandInteraction writer, params Token[] tokens)
        {
            if(tokens.Length == 1 && tokens[0] is VariableToken)
            {
                var macroLines = GetVariable(tokens[0] as VariableToken).GetObjectValue().ToString().Split('\n');
                foreach(var line in macroLines)
                {
                    if(!monitor.Parse(line, writer))
                    {
                        throw new RecoverableException(string.Format("Parsing line '{0}' failed.", line));
                    }
                }
            }
            else
            {
                if(!monitor.ParseTokens(tokens, writer))
                {
                    throw new RecoverableException("Parsing failed.");
                }
            }
        }

        public ExecuteCommand(Monitor monitor, string name, string noun, Func<VariableToken, Token> getVariable, Func<IEnumerable<string>> getVariables):base(monitor, name, "executes a command or the content of a {0}.".FormatWith(noun))
        {
            GetVariable = getVariable;
            GetVariables = getVariables;
            this.noun = noun;
        }

        private readonly string noun;
        private readonly Func<VariableToken, Token> GetVariable;
        private readonly Func<IEnumerable<string>> GetVariables;
    }
}

