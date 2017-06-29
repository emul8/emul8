//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.UserInterface;

namespace Emul8.Plugins.TracePlugin
{
    [Plugin(Name = "tracer", Description = "Tracing plugin", Version = "0.1", Vendor = "Antmicro")]
    public class TracePlugin : IDisposable
    {
        public TracePlugin(Monitor monitor)
        {
            this.monitor = monitor;           
            traceCommand = new TraceCommand(monitor);
            monitor.RegisterCommand(traceCommand);
        }

        public void Dispose()
        {
            monitor.UnregisterCommand(traceCommand);
        }

        private readonly TraceCommand traceCommand;
        private readonly Monitor monitor;
    }
}

