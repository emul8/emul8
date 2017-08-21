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
    [ConsoleBackendAnalyzerProvider("GnomeTerminal")]
    public class GnomeTerminalProvider : ProcessBasedProvider
    {
        protected override Process CreateProcess(string consoleName, string command)
        {
            var p = new Process();
            p.EnableRaisingEvents = true;
            var position = WindowPositionProvider.Instance.GetNextPosition();

            var arguments = string.Format("--tab -e \"{3}\" --title '{0}' --geometry=+{1}+{2}", consoleName, (int)position.X, (int)position.Y, command);
            p.StartInfo = new ProcessStartInfo("gnome-terminal", arguments)
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
                    LogError("gnome-terminal", arguments, proc.ExitCode);
                }
                // We do not call InnerOnClose here, because gnome-terminal closes immediately after spawning new window.
            };
            return p;
        }
    }
}

#endif
