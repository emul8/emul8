//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using Emul8.Utilities;

namespace Emul8.CLI 
{
    public class CrashHandler
    {
        public void HandleCrash(UnhandledExceptionEventArgs eObject)
        {
            var e = (Exception)eObject.ExceptionObject;
            var ex = e;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Fatal error:");
            while(ex != null)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                ex = ex.InnerException;
                if(ex != null)
                {
                    Console.Error.WriteLine("Inner exception:");
                }
            }
            Console.ResetColor();
            var path = TemporaryFilesManager.Instance.EmulatorTemporaryPath + TemporaryFilesManager.CrashSuffix;
            Directory.CreateDirectory(path);
            var filename = CustomDateTime.Now.ToString("yyyyMMddHHmmssfff");
            ex = e;
            using(var file = File.CreateText(Path.Combine(path, filename)))
            {
                while(ex != null)
                {
                    file.WriteLine(ex.Message);
                    file.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                    if(ex != null)
                    {
                        file.WriteLine("Inner exception:");
                    }
                }
            }

            Environment.Exit(-1);
        }
    }
}

