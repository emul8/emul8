//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.OptionsParser;

namespace Emul8.CLI 
{
	internal class Options
	{
        [Name('c', "console"), DefaultValue(false), Description("Set the output for console window instead of many terminals.")]
        public bool ConsoleMode { get; set; }

        [Name('s', "stdinout"), DefaultValue(false), Description("Read input from standard input and write to standard output - for use in pipes (this option is exclusive with -e and startup script passed as an argument).")]
        public bool StdInOut { get; set; }

        [Name('p', "plain"), DefaultValue(false), Description("Remove steering codes (e.g., colours) from output.")]
        public bool Plain { get; set; }

        [Name('P', "port"), DefaultValue(-1), Description("Instead of opening a window listen for monitor commands on port.")]
        public int Port { get; set; }

        [Name('e', "execute"), Description("Execute command on startup (this option is exclusive with -s and startup script passed as an argument).")]
        public string Execute { get; set; }

        [Name("script"), PositionalArgument(0)]
        public string ScriptPath { get; set; }

        public bool Validate(out string error)
        {
            if(!string.IsNullOrEmpty(ScriptPath) && (StdInOut || !string.IsNullOrEmpty(Execute)))
            {
                error = "Script path and execute command cannot be set at the same time";
                return false;
            }

            error = null;
            return true;
        }
	}
}

