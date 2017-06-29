//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Threading;
using Emul8.Logging;

namespace Emul8.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConsoleBackend.Instance.WindowTitle = "Emul8";

            var thread = new Thread(() =>
            {
                CommandLineInterface.Run(args);
                Emulator.FinishExecutionAsMainThread();
            });
            thread.Start();
            Emulator.ExecuteAsMainThread();
        }
    }
}
