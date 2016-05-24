//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading;
using Emul8.Core;
using Emul8.Exceptions;
using Emul8.Logging;
using Emul8.UserInterface;
using System.Collections.Concurrent;
using Emul8.Utilities;

namespace Emul8
{
    public static class Emulator
    {
        public static void Exit()
        {
            var beforeExit = BeforeExit;
            if(beforeExit != null)
            {
                beforeExit();
            }
            exitEvent.Set();
        }

        public static void WaitForExit()
        {
            exitEvent.Wait();
        }

        public static void DisposeAll()
        {
            EmulationManager.Instance.Clear();
            TypeManager.Instance.Dispose();
            Logger.Dispose();
        }

        public static IUserInterfaceProvider UserInterfaceProvider
        {
            get
            {
                if(userInterfaceProvider == null)
                {
                    throw new RecoverableException("User interface provider not set");
                }
                return userInterfaceProvider;
            }
            set
            {
                userInterfaceProvider = value;
            }
        }

        public static void ExecuteOnMainThread(Action what)
        {
            actionsOnMainThread.Add(what);
        }

        public static void ExecuteAsMainThread()
        {
            Action action;
            while(actionsOnMainThread.TryTake(out action, -1))
            {
                action();
            }
        }

        public static void FinishExecutionAsMainThread()
        {
            actionsOnMainThread.CompleteAdding();
        }

        private static readonly BlockingCollection<Action> actionsOnMainThread = new BlockingCollection<Action>();

        public static event Action BeforeExit;
        private static IUserInterfaceProvider userInterfaceProvider;
        private static ManualResetEventSlim exitEvent = new ManualResetEventSlim();
    }
}

