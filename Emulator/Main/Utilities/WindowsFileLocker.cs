//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
#if PLATFORM_WINDOWS
using System;
using System.IO;
using System.Threading;
using Emul8.Logging;

namespace Emul8.Utilities
{
    public class WindowsFileLocker : IDisposable
    {
        public WindowsFileLocker(string fileToLock)
        {
            path = fileToLock;
            var counter = 0;
            while(true)
            {
                try
                {
                    file = File.Open(fileToLock, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    return;
                }
                catch(IOException)
                {
                    // ignore exception
                }

                Thread.Sleep(500);
                counter++;
                if(counter == 10)
                {
                    counter = 0;
                    Logger.Log(LogLevel.Warning, "Still trying to lock file {0}", fileToLock);
                }
            }
        }

        public void Dispose()
        {
            file.Close();
            File.Delete(path);
        }

        private readonly FileStream file;
        private readonly string path;
    }
}
#endif
