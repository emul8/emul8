//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using System.Threading;
using Antmicro.OptionsParser;
using Emul8.Utilities;
using Emul8.Logging;

namespace Emul8.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConsoleBackend.Instance.WindowTitle = "Emul8";
            ConfigureEnvironment();

            var thread = new Thread(() =>
            {
                var options = new Options();
                var optionsParser = new OptionsParser();
                if(!optionsParser.Parse(options, args))
                {
                    return;
                }

                CommandLineInterface.Run(options);
                Emulator.FinishExecutionAsMainThread();
            });
            thread.Start();
            Emulator.ExecuteAsMainThread();
        }

        private static void ConfigureEnvironment()
        {
            string emul8Dir;
            if(Misc.TryGetEmul8Directory(out emul8Dir))
            {
                var localConfig = Path.Combine(emul8Dir, "emul8.config");
                if(File.Exists(localConfig))
                {
                    ConfigurationManager.Initialize(localConfig);
                }
            }
        }
    }
}
