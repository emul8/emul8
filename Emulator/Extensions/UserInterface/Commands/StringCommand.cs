//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;
using System.Linq;

namespace Emul8.UserInterface.Commands
{
    public class StringCommand : AutoLoadCommand
    {
        /*[Runnable]
        public void Run(ICommandInteraction writer, params Token[] tokens)
        {
            writer.WriteLine("\"" + string.Join("", tokens.Select(x=>x.GetObjectValue().ToString())) + "\"");
        }*/

        [Runnable]
        public void Run(ICommandInteraction writer, Token[] tokens)
        {
            writer.WriteLine("\"" + string.Join(" ", tokens.Select(x=>x.GetObjectValue().ToString())) + "\"");
        }

        public StringCommand(Monitor monitor) : base(monitor, "string", "treat given arguments as a single string.", "str") 
        {
        }
    }
}

