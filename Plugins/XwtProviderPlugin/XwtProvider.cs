//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using Emul8.Plugins;
using Emul8;
using System;
using Xwt;
using Xwt.GtkBackend;
using System.Threading;
using Emul8.Exceptions;
using Emul8.UserInterface;
using Emul8.Logging;

namespace Emul8.Plugins.XwtProviderPlugin
{
    [Plugin(Name = "XwtProvider", Version = "0.1", Description = "Xwt provider plugin", Vendor = "Antmicro", Modes = new [] { "CLI" })]
    public class XwtProvider : IDisposable
    {
        static XwtProvider()
        {
            internalLock = new object();
            UiThreadId = -1;
        }
        
        public static void InitializeXwt()
        {
            Application.Initialize(ToolkitType.Gtk);
        }
        
        public static void RunXwtInCurrentThread()
        {
            lock(internalLock)
            {
                if(UiThreadId != -1)
                {
                    throw new ArgumentException(string.Format("UI thread is already running: {0}", UiThreadId));
                }
                UiThreadId = Thread.CurrentThread.ManagedThreadId;
            }
                
            Application.UnhandledException += LocalCrashHandler;
            GLib.ExceptionManager.UnhandledException += arg => CrashHandler.HandleCrash((Exception)arg.ExceptionObject);
            Application.Run();
            GtkTextLayoutBackendHandler.DisposeResources();
            
            lock(internalLock)
            {
                UiThreadId = -1;
            }
        }
        
        public static int UiThreadId { get; private set; }
        
        public XwtProvider()
        {
            if(UiThreadId != -1)
            {
                // if there is an UI thread running already then do nothing
                return;
            }
               
            try 
            {
                previousProvider = Emulator.UserInterfaceProvider;
            } 
            catch
            {
                previousProvider = null;
            }
            
            Emulator.UserInterfaceProvider = new WindowedUserInterfaceProvider();
            StartXwtThread();
        }

        public void Dispose()
        {
            Emulator.UserInterfaceProvider = previousProvider;
            StopXwtThread();
        }
        
        private static void LocalCrashHandler(object sender, ExceptionEventArgs args)
        {
            var exception = args.ErrorException;
            var recoverable = exception as RecoverableException;
            if(recoverable != null)
            {
                MessageDialog.ShowError(args.ErrorException.Message);
            }
            else
            {
                MessageDialog.ShowWarning(args.ErrorException.ToString());
            }           
        }
        
        private static object internalLock;
        
        private void StartXwtThread()
        {
            Emulator.ExecuteOnMainThread(() =>
            {
                InitializeXwt();
                RunXwtInCurrentThread();
            });
        }
        
        private void StopXwtThread()
        {
            lock(internalLock)
            {
                if(UiThreadId == -1)
                {
                    return;
                }
                ApplicationExtensions.InvokeInUIThreadAndWait(Application.Exit);
                UiThreadId = -1;
            }
        }
        
        private readonly IUserInterfaceProvider previousProvider;
    }
}

