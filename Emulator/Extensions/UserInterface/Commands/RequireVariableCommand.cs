//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.UserInterface.Tokenizer;

namespace Emul8.UserInterface.Commands
{
    public class RequireVariableCommand : AutoLoadCommand
    {
        [Runnable]
        public void Run(Token token)
        {
            // THIS METHOD IS INTENTIONALY LEFT BLANK
        }

        public RequireVariableCommand(Monitor monitor) : base(monitor, "require", "verifies the existence of a variable.")
        {
        }
    }
}

