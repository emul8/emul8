//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading.Tasks;
using Emul8.CLI;
using Emul8.Core;
using Emul8.Peripherals.UART;
using Emul8.Robot;
using Emul8.Utilities;

namespace Emul8.RobotFrontend
{
    public class RobotFrontend
    {
        public static void Main(string[] args)
        {
            int port;
            if(args.Length != 1 || !int.TryParse(args[0], out port))
            {
                Console.Error.WriteLine("Provide a valid port number as an argument.");
                return;
            }

            var keywordManager = new KeywordManager();
            TypeManager.Instance.AutoLoadedType += keywordManager.Register;

            var processor = new XmlRpcServer(keywordManager);
            server = new HttpServer(processor);

            var preferredUARTAnalyzer = typeof(UARTWindowBackendAnalyzer);
            EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
            EmulationManager.Instance.EmulationChanged += () =>
            {
                EmulationManager.Instance.CurrentEmulation.BackendManager.SetPreferredAnalyzer(typeof(UARTBackend), preferredUARTAnalyzer);
            };

            Task.Run(() => 
            {
                using(var xwt = new XwtProvider(new WindowedUserInterfaceProvider()))
                {
                    server.Run(port);
                    server.Dispose();
                }
            });

            Emulator.ExecuteAsMainThread();
        }

        public static void ExecuteKeyword(string name, string[] arguments)
        {
            server.Processor.RunKeyword(name, arguments);
        }

        public static void Shutdown()
        {
            server.Shutdown();
            Emulator.FinishExecutionAsMainThread();
        }

        private static HttpServer server;
    }
}
