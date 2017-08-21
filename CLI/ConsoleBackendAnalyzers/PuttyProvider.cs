//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
#if EMUL8_PLATFORM_LINUX
using System.Diagnostics;

namespace Emul8.CLI
{
    [ConsoleBackendAnalyzerProvider("Putty")]
    public class PuttyProvider : ProcessBasedProvider
    {
        protected override Process CreateProcess(string consoleName, string command)
        {
            var p = new Process();
            p.EnableRaisingEvents = true;
            var arguments = string.Format("{0} -serial -title '{0}'", consoleName);
            p.StartInfo = new ProcessStartInfo("putty", arguments)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            p.Exited += (sender, e) =>
            {
                var proc = sender as Process;
                if (proc.ExitCode != 0)
                {
                    LogError("Putty", arguments, proc.ExitCode);
                }
                InnerOnClose();
            };

            return p;
        }
    }
}

#endif
