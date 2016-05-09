//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.OptionsParser;

namespace Emul8.Launcher
{
    public class BasicOptions
    {
        [Name("root-path"), Description("Search for binaries in this directory."), DefaultValue(".")]
        public string RootPath { get; set; }
    }

    public class Options : BasicOptions, IValidatedOptions
    {
        [Name('d', "debug"), Description("Use non-optimized, debuggable version.")]
        public bool Debug { get; set; }

        [Description("Do not output errors on console.")]
        public bool Quiet { get; set; }

        [Name("external-debugger-port"), Description("Listen for external debugger."), DefaultValue(-1)]
        public int DebuggerSocketPort { get; set; }

        public bool Validate(out string error)
        {
            if(!Debug && DebuggerSocketPort != -1)
            {
                error = "Debugger is not available in 'Release' mode";
                return false;
            }

            error = null;
            return true;
        }
    }
}

