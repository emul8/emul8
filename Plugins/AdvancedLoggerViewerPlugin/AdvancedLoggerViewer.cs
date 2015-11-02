//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Emul8.Plugins;
using Emul8.UserInterface;
using Emul8.Plugins.XwtProviderPlugin;
using Emul8.Logging.Backends;

namespace Emul8.Plugins.AdvancedLoggerViewer
{
    [Plugin(Name = "AdvancedLoggerViewer", Version = "0.1", Description = "Viewer for advanced logger", Vendor = "Antmicro", Dependencies = new [] { typeof(XwtProvider) })]
    public class AdvancedLoggerViewer : IDisposable
    {
        public AdvancedLoggerViewer(Monitor monitor)
        {
            this.monitor = monitor;
            command = new AdvancedLoggerViewerCommand(monitor);
            monitor.RegisterCommand(command);
            
            // start lucene backend as soon as possible
            // in order to gather more log entries
            LuceneLoggerBackend.EnsureBackend();
        }
        
        public void Dispose()
        {
            monitor.UnregisterCommand(command);
        }
        
        private readonly Monitor monitor;
        private readonly AdvancedLoggerViewerCommand command;
    }
}

