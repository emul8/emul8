//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Xwt;
using System.Threading;
using System.Collections.Concurrent;
using Emul8.Exceptions;

namespace Emul8.CLI
{
    public static class ApplicationExtensions
    {
        static ApplicationExtensions()
        {
            actionsToRunInUIThread = new BlockingCollection<Action>();
            var t = new Thread(() =>
            {
                while(true)
                {
                    var a = actionsToRunInUIThread.Take();
                    Application.Invoke(a);
                }
            });
            t.IsBackground = true;
            t.Name = "ApplicationExtensions GUI invoker";
            t.Start();
        }

        public static void InvokeInUIThread(Action action)
        {
            CheckXwtStatus();
            if(Thread.CurrentThread.ManagedThreadId == XwtProvider.UiThreadId)
            {
                action();
            }
            else
            {
                Application.Invoke(action);
            }
        }

        public static void InvokeInUIThreadNonBlocking(Action action)
        {
            CheckXwtStatus();
            if(Thread.CurrentThread.ManagedThreadId == XwtProvider.UiThreadId)
            {
                actionsToRunInUIThread.Add(action);
            }
            else
            {
                Application.Invoke(action);
            }
        }

        public static T InvokeInUIThreadAndWait<T>(Func<T> function)
        {
            CheckXwtStatus();
            if(Thread.CurrentThread.ManagedThreadId == XwtProvider.UiThreadId)
            {
                return function();
            }

            T result = default(T);
            var mre = new ManualResetEventSlim();

            Application.Invoke(() =>
            {
                result = function();
                mre.Set();
            });

            mre.Wait();
            return result;
        }

        public static void InvokeInUIThreadAndWait(Action action)
        {
            CheckXwtStatus();
            if(Thread.CurrentThread.ManagedThreadId == XwtProvider.UiThreadId)
            {
                action();
                return;
            }

            var mre = new ManualResetEventSlim();
            Application.Invoke(() =>
            {
                action();
                mre.Set();
            });

            mre.Wait();
        }

        private static void CheckXwtStatus()
        {
            if(XwtProvider.UiThreadId == -1)
            {
                throw new RecoverableException("An action requiring GUI environment detected, but the XWT provider is not started.");
            }
        }

        private static BlockingCollection<Action> actionsToRunInUIThread = new BlockingCollection<Action>();
    }
}

