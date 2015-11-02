//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;
using Emul8.Core;
using Emul8.UserInterface;
using Emul8.Backends.Terminals;
using System.Threading;
using AntShell;
using AntShell.Terminal;
using Emul8.Exceptions;
using Emul8.Utilities;
using Emul8.Peripherals.UART;
using System.Linq;
using Antmicro.OptionsParser;

namespace Emul8.CLI
{
    public static class CommandLineInterface
    {
        public static void Run(string[] args)
        {
            var options = new Options();
            var optionsParser = new OptionsParser();
            if(!optionsParser.Parse(options, args))
            {
                return;
            }

            using(var context = ObjectCreator.OpenContext())
            {
                var monitor = new Emul8.UserInterface.Monitor();
                context.RegisterSurrogate(typeof(Emul8.UserInterface.Monitor), monitor);

                // we must initialize plugins AFTER registering monitor surrogate
                // as some plugins might need it for construction
                TypeManager.Instance.PluginManager.Init("CLI");

                Logger.AddBackend(ConsoleBackend.Instance, "console");

                EmulationManager.Instance.ProgressMonitor.Handler = new CLIProgressMonitor();

                var crashHandler = new CrashHandler();
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => crashHandler.HandleCrash(e);

                var resetEvent = new ManualResetEventSlim();
                if(options.StdInOut)
                {
                    Logger.AddBackend(new FileBackend("logger.log"), "file");
                    var world = new StreamIOSource(Console.OpenStandardInput(), Console.OpenStandardOutput());
                    var io = new DetachableIO(world);
                    monitor.Quitted += resetEvent.Set;
                    var handler = new StdInOutHandler(io, monitor, options.Plain);
                    handler.Start();
                }
                else
                {
                    Shell shell = null;
                    Type preferredUARTAnalyzer = null;
                    if(options.Port > 0)
                    {
                        var io = new DetachableIO(new SocketIOSource(options.Port));
                        shell = ShellProvider.GenerateShell(io, monitor, true, false);
                        shell.Quitted += resetEvent.Set;
                    }
                    else if(options.ConsoleMode)
                    {
                        preferredUARTAnalyzer = typeof(UARTMultiplexedBackendAnalyzer);

                        var stream = new PtyUnixStream("/proc/self/fd/1");
                        var world = new StreamIOSource(stream, stream.Name);

                        Logger.AddBackend(new FileBackend("logger.log"), "file");
                        var consoleTerm = new ConsoleTerminal(world);
                        UARTMultiplexedBackendAnalyzer.Multiplexer = consoleTerm;
                   
                        monitor.Console = consoleTerm;

                        shell = ShellProvider.GenerateShell(monitor);
                        consoleTerm.AttachTerminal("shell", shell.Terminal);

                        shell.Quitted += resetEvent.Set;
                    }
                    else
                    {
                        preferredUARTAnalyzer = typeof(UARTWindowBackendAnalyzer);

                        var stream = new PtyUnixStream();
                        var dio = new DetachableIO(new StreamIOSource(stream, stream.Name));
                        var terminal = new UARTWindowBackendAnalyzer(dio);
                        shell = ShellProvider.GenerateShell(dio, monitor);     

                        shell.Quitted += resetEvent.Set;
                        monitor.Quitted += shell.Stop;

                        try
                        {
                            terminal.Show();
                        }
                        catch(InvalidOperationException ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine(ex.Message);
                            resetEvent.Set();
                        }
                    }

                    if(preferredUARTAnalyzer != null)
                    {
                        EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
                        EmulationManager.Instance.EmulationChanged += () =>
                        {
                            EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
                        };
                    }
                    monitor.Interaction = shell.Writer;
                    monitor.UseConsole = options.ConsoleMode;
                    monitor.MachineChanged += emu => shell.SetPrompt(emu != null ? new Prompt(string.Format("({0}) ", emu), ConsoleColor.DarkYellow) : null);

                    if(options.Execute != null)
                    {
                        shell.Started += s => s.InjectInput(string.Format("{0}\n", options.Execute));
                    }
                    else if(!string.IsNullOrEmpty(options.ScriptPath))
                    {
                        shell.Started += s => s.InjectInput(string.Format("i $CWD/{0}\n", options.ScriptPath));
                    }

                    new Thread(x => shell.Start(true)) { IsBackground = true, Name = "Shell thread" }.Start();
                }

                resetEvent.Wait();
                EmulationManager.Instance.Clear();
                TypeManager.Instance.Dispose();
                Logger.Dispose();
            }
        }
    }
}
