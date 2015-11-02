//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;

namespace Emul8.Utilities
{
    public class PosixFileLocker : IDisposable
    {
        public PosixFileLocker(string fileToLock, bool ensureFileExistence = false)
        {
            file = fileToLock;
            if(!File.Exists(file))
            {
                if(!ensureFileExistence)
                {
                    throw new ArgumentException("File {0} does not exist.".FormatWith(file));
                }
                using(File.Create(file))
                {
                }
            }
            if(!Misc.TryLockFile(file, out fd))
            {
                throw new InvalidOperationException("File {0} not locked.".FormatWith(file));
            }
        }

        public void Dispose()
        {
            if(!Misc.TryUnlockFile(fd))
            {
                throw new InvalidOperationException("File {0} not unlocked.".FormatWith(file));
            }
        }

        private readonly int fd;
        private readonly string file;

    }
}

