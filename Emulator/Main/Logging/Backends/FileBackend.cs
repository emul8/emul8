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
using Emul8.Utilities;

namespace Emul8.Logging
{
    public class FileBackend : TextBackend 
    {
        public FileBackend(string filePath)
        {
            var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            output = new StreamWriter(stream);
            sync = new object();
            timer = new Timer(x => Flush(), null, 0, 5000);
        }

        public override void Log(LogEntry entry)
        {
            if(!ShouldBeLogged(entry))
            {
                return;
            }

            lock(sync)
            {
                if(isDisposed)
                {
                    return;
                }

                var type = entry.Type;
                var message = FormatLogEntry(entry);
                output.WriteLine(string.Format("{0:HH:mm:ss} [{1}] {2}", CustomDateTime.Now, type, message));
            }
        }

        public override void Dispose()
        {
            lock(sync)
            {
                timer.Dispose();
                if(isDisposed)
                {
                    return;
                }
                output.Dispose();
                isDisposed = true;
            }
        }

        public void Flush()
        {
            if(!Monitor.TryEnter(sync))
            {
                return;
            }
            try
            {
                output.Flush();
            }
            finally
            {
                Monitor.Exit(sync);
            }
        }
            
        private bool isDisposed;
        private readonly Timer timer;
        private readonly object sync;
        private readonly TextWriter output;
    }
}

