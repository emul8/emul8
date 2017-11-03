//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
#if PLATFORM_OSX
using System.Diagnostics;
using Emul8.Utilities;
using System.IO;
using Mono.Unix.Native;

namespace Emul8.CLI
{
    [ConsoleBackendAnalyzerProvider("TerminalApp")]
    public class TerminalAppProvider : ProcessBasedProvider
    {
        protected override Process CreateProcess(string consoleName, string command)
        {
            var script = TemporaryFilesManager.Instance.GetTemporaryFile();
            File.WriteAllLines(script, new [] {
                "#!/bin/bash",
                command
            });

            var p = new Process();
            p.EnableRaisingEvents = true;
            Syscall.chmod(script, FilePermissions.S_IXUSR | FilePermissions.S_IRUSR | FilePermissions.S_IWUSR);

            var arguments = $"-a /Applications/Utilities/Terminal.app {script}";
            p.StartInfo = new ProcessStartInfo("open", arguments)
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
                    LogError("Terminal.app", arguments, proc.ExitCode);
                }
                // We do not call InnerOnClose here, because the closing routine of the "open" application is counterintuitive.
                // In current setup it closes automatically like gnome-terminal. We may add -W or -Wn, but
                // then Exited event never gets called on window close, the user must close the app from the
                // Dock. This will either force the user to kill all terminals (-W) or it will create multiple
                // terminal icons (-Wn) that will stay there unless manually closed. Both options are bad.
            };
            return p;
        }
    }
}
#endif
