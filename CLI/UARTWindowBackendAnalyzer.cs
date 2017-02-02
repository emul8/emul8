//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;
using Emul8.Peripherals.UART;
using Emul8.Logging;
using Emul8.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Emul8.Exceptions;
using System.ComponentModel;
using AntShell.Terminal;
using System.Threading;
#if !EMUL8_PLATFORM_WINDOWS
using Mono.Unix.Native;
#endif
using TermSharp.Vt100;
using Xwt;
using Emul8.Core;

namespace Emul8.CLI
{
    public class UARTWindowBackendAnalyzer : IAnalyzableBackendAnalyzer<UARTBackend>
    {
        // this constructor is needed by the monitor; do not remove it
        public UARTWindowBackendAnalyzer()
        {
#if EMUL8_PLATFORM_WINDOWS
            preferredTerminal = ConfigurationManager.Instance.Get("general", "terminal", TerminalTypes.Termsharp);
            if(preferredTerminal != TerminalTypes.Termsharp)
            {
                Logger.LogAs(this, LogLevel.Warning, "Only >>Termsharp<< terminal is available on Windows - forcing to use it.");
            }
#else
            preferredTerminal = ConfigurationManager.Instance.Get("general", "terminal", TerminalTypes.XTerm);
            if(preferredTerminal == TerminalTypes.Termsharp)
            {
#endif
                ApplicationExtensions.InvokeInUIThreadAndWait(() => {
                    terminalWidget = new TerminalWidget(() => window.HasFocus);
                });
                IO = terminalWidget.IO;
#if !EMUL8_PLATFORM_WINDOWS
            }
            else
            {
                ptyUnixStream = new PtyUnixStream();
                IO = new IOProvider(new StreamIOSource(ptyUnixStream));
            }
#endif
        }

        public void AttachTo(UARTBackend backend)
        {
            backend.BindAnalyzer(IO);
            Backend = backend;

            string uartName;
            if(EmulationManager.Instance.CurrentEmulation.TryGetEmulationElementName(backend.UART, out uartName))
            {
                Name = uartName;
            }
        }

        public void Show()
        {
#if !EMUL8_PLATFORM_WINDOWS
            if(terminalWidget != null)
            {
#endif
                ApplicationExtensions.InvokeInUIThreadAndWait(() => {
                    window = new Window();
                    window.Title = Name;
                    window.Width = 700;
                    window.Height = 400;
                    window.Padding = new WidgetSpacing();
                    window.Content = terminalWidget;
                    window.Show();
                    if(NextWindowLocation != default(Point))
                    {
                        window.Location = NextWindowLocation;
                        NextWindowLocation = NextWindowLocation.Offset(WindowOffset);
                    }
                    else
                    {
                        NextWindowLocation = window.Location.Offset(WindowOffset);
                    }
                });
#if !EMUL8_PLATFORM_WINDOWS
            }
            else
            {
                var windowCreators = new Dictionary<TerminalTypes, CreateWindowDelegate>
                {
                    {TerminalTypes.XTerm, CreateXtermWindow},
                    {TerminalTypes.Putty, CreatePuttyWindow},
                    {TerminalTypes.GnomeTerminal, CreateGnomeTerminalWindow},
                    {TerminalTypes.TerminalApp, CreateTerminalAppWindow}
                };

                var commandString = string.Format("screen {0}", ptyUnixStream.Name);
                //Try preferred terminal first, than any other. If all fail, throw.
                if(!windowCreators.OrderByDescending(x => x.Key == preferredTerminal).Any(x => x.Value(commandString, out process)))
                {
                    throw new NotSupportedException(String.Format("Could not start terminal. Possible config values: {0}",
                        windowCreators.Keys.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y)));
                }

                Thread.Sleep(1000);
                // I know - this is ugly. But here's the problem:
                // we start terminal process with embedded socat and it takes time
                // how much? - you ask
                // good question - we don't know it; sometimes more, sometimes less
                // sometimes it causes a problem - socat is not ready yet when first data arrives
                // what happens then? - you ask
                // good question - we lost some input, Emul8 banner most probably
                // how to solve it? - you ask
                // good question - with no good answer though, i'm affraid
                // that is why we sleep here for 1s hoping it's enough
                //
                // This will be finally changed to our own implementation of VirtualTerminalEmulator.
            }
#endif
        }

        public void Hide()
        {
            var w = window;
            if(w != null)
            {
                ApplicationExtensions.InvokeInUIThreadAndWait(() =>
                {
                    w.Hide();
                });
                w = null;
                return;
            }

            var p = process;
            if(p == null)
            {
                return;
            }

            try
            {
                p.CloseMainWindow();
            }
            catch(InvalidOperationException e)
            {
                // do not report an exception if the problem has already exited
                if(!e.Message.Contains("finished"))
                {
                    throw;
                }
            }
            process = null;
        }

        public string Name { get; set; }

        public IAnalyzableBackend Backend { get; private set; }

        public IOProvider IO { get; private set; }

        private static Tuple<int, int> GetNextWindowPosition()
        {
            lock(StartingPosition)
            {
                var result = StartingPosition;
                var newWidth = StartingPosition.Item1 + WindowWidth;
                var newHeight = StartingPosition.Item2;
                if(newWidth > MaxWidth)
                {
                    newWidth = 10;
                    newHeight += WindowHeight;
                }
                StartingPosition = Tuple.Create(newWidth, newHeight);
                return result;
            }
        }

        private static Tuple<int, int> StartingPosition = Tuple.Create(10, 10);

#if !EMUL8_PLATFORM_WINDOWS
        private bool CreateGnomeTerminalWindow(string command, out Process p)
        {
            try
            {
                p = new Process();
                p.EnableRaisingEvents = true;
                var position = GetNextWindowPosition();

                var arguments = string.Format("--tab -e \"{3}\" --title '{0}' --geometry=+{1}+{2}", Name, position.Item1, position.Item2, command);
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
                };
                p.Start();
                Logger.LogAs(this, LogLevel.Info, "Terminal shown");
                return true;
            }
            catch(Win32Exception)
            {
            }
            p = null;
            return false;
        }

        private bool CreatePuttyWindow(string arg, out Process p)
        {
            try
            {
                p = new Process();
                p.EnableRaisingEvents = true;
                var arguments = string.Format("{0} -serial -title '{0}'", Name);
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
                };
                p.Start();
                return true;
            }
            catch(Win32Exception)
            {
            }
            p = null;
            return false;
        }

        private bool CreateXtermWindow(string cmd, out Process p)
        {
            try
            {
                p = new Process();
                var position = GetNextWindowPosition();
                var minFaceSize = @"XTerm.*.faceSize1: 6";
                var keys = @"XTerm.VT100.translations: #override \\n" +
                           // disable menu on CTRL click
                           @"!Ctrl <Btn1Down>: ignore()\\n" +
                           @"!Ctrl <Btn2Down>: ignore()\\n" +
                           @"!Ctrl <Btn3Down>: ignore()\\n" +
                           @"!Lock Ctrl <Btn1Down>: ignore()\\n" +
                           @"!Lock Ctrl <Btn2Down>: ignore()\\n" +
                           @"!Lock Ctrl <Btn3Down>: ignore()\\n" +
                           @"!@Num_Lock Ctrl <Btn1Down>: ignore()\\n" +
                           @"!@Num_Lock Ctrl <Btn2Down>: ignore()\\n" +
                           @"!@Num_Lock Ctrl <Btn3Down>: ignore()\\n" +
                           @"!Lock Ctrl @Num_Lock <Btn1Down>: ignore()\\n" +
                           @"!Lock Ctrl @Num_Lock <Btn2Down>: ignore()\\n" +
                           @"!Lock Ctrl @Num_Lock <Btn3Down>: ignore()\\n" +
                           // change default font size change keys into CTRL +/-
                           @"Shift~Ctrl <KeyPress> KP_Add:ignore()\\n" +
                           @"Shift Ctrl <KeyPress> KP_Add:ignore()\\n" +
                           @"Shift <KeyPress> KP_Subtract:ignore()\\n" +
                           @"Ctrl <KeyPress> KP_Subtract:smaller-vt-font()\\n" +
                           @"Ctrl <KeyPress> KP_Add:larger-vt-font() \\n"; 
                var scrollKeys = @"XTerm.VT100.scrollbar.translations: #override \\n"+
                                 @"<Btn5Down>: StartScroll(Forward) \\n"+
                                 @"<Btn1Down>: StartScroll(Continuous) MoveThumb() NotifyThumb() \\n"+
                                 @"<Btn4Down>: StartScroll(Backward) \\n"+
                                 @"<Btn3Down>: StartScroll(Continuous) MoveThumb() NotifyThumb() \\n"+
                                 @"<Btn2Down>: ignore() \\n"+
                                 @"<Btn1Motion>: MoveThumb() NotifyThumb() \\n"+
                                 @"<BtnUp>: NotifyScroll(Proportional) EndScroll()";
                var fonts = "DejaVu Sans Mono, Ubuntu Sans Mono, Droid Sans Mono";

                var command = string.Format(@"-T '{0}' -sb -rightbar -xrm '*Scrollbar.thickness: 10' -xrm '*Scrollbar.background: #CCCCCC' -geometry +{1}+{2}  -xrm '*Scrollbar.foreground: #444444' -xrm 'XTerm.vt100.background: black' -xrm 'XTerm.vt100.foreground: white' -fa '{3}' -fs 10 -xrm '{4}' -xrm '{5}' -xrm '{6}' -e {7}", 
                    Name, position.Item1, position.Item2, fonts, keys, minFaceSize, scrollKeys, cmd);

                p.StartInfo = new ProcessStartInfo("xterm", command) 
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                };
                p.EnableRaisingEvents = true;
                p.Exited += (sender, e) => 
                { 
                    var proc = sender as Process;
                    if (proc.ExitCode != 0)
                    {
                        LogError("Xterm", command, proc.ExitCode);
                    }
                };
                p.Start();
                Logger.LogAs(this, LogLevel.Info, "Terminal shown");
                return true;
            }
            catch(Win32Exception)
            {
            }
            p = null;
            return false;
        }

        private bool CreateTerminalAppWindow(string arg, out Process p)
        {
            var script = TemporaryFilesManager.Instance.GetTemporaryFile();
            File.WriteAllLines(script, new [] {
                "#!/bin/bash",
                string.Format("/usr/bin/screen {0}", ptyUnixStream.Name)
            });

            try
            {
                p = new Process();
                p.EnableRaisingEvents = true;
                Syscall.chmod(script, FilePermissions.S_IXUSR | FilePermissions.S_IRUSR | FilePermissions.S_IWUSR);

                var arguments = string.Format("{0} {1}", "-a /Applications/Utilities/Terminal.app", script);
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
                };
                p.Start();
                return true;
            }
            catch(Win32Exception)
            {
            }
            p = null;
            return false;
        }
#endif

        private void LogError(string source, string arguments, int exitCode)
        {
            Logger.LogAs(this, LogLevel.Error, "There was an error while starting {2} with arguments: {0}. It exited with code: {1}. In order to use different terminal change preferences in configuration file.", arguments, exitCode, source);
        }

        private delegate bool CreateWindowDelegate(string command, out Process process);

        private Process process;
        private Xwt.Window window;
        private TerminalWidget terminalWidget;

        private readonly PtyUnixStream ptyUnixStream;
        private readonly TerminalTypes preferredTerminal;

        private static Point NextWindowLocation;
        private static readonly Point WindowOffset = new Point(30, 50);

        private const int WindowHeight = 500;
        private const int WindowWidth = 670;
        private const int MaxWidth = 1700;

        private enum TerminalTypes
        {
            Putty,
            XTerm,
            GnomeTerminal,
            TerminalApp,
            Termsharp
        }
    }
}

