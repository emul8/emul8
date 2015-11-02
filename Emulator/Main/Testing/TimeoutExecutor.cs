//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading;

namespace Emul8.Testing
{
    public static class TimeoutExecutor
    {
        public static T Execute<T>(Func<T> func, int timeout)
        {
            T result;
            TryExecute(func, timeout, out result);
            return result;
        }

        public static bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            T res = default(T);
            var thread = new Thread(() => res = func())
            {
                IsBackground = true,
                Name = typeof(TimeoutExecutor).Name
            };
            thread.Start();
            var finished = thread.Join(timeout);
            if (!finished)
            {
                thread.Abort();
            }
            result = res;
            return finished;
        }

        public static bool WaitForEvent(ManualResetEvent e, int timeout)
        {
            return e.WaitOne(timeout);
        }
    }
}

