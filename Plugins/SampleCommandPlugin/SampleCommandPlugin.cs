//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Emul8.Plugins;
using Emul8;
using Emul8.UserInterface;

namespace Emul8.Plugins.SampleCommandPlugin
{
    [Plugin(Name = "Sample command plugin", Version = "1.0", Description = "Sample plugin providing \"hello\" command.", Vendor = "Antmicro")]
    public sealed class SampleCommandPlugin : IDisposable
    {
        public SampleCommandPlugin(Monitor monitor)
        {
            this.monitor = monitor;            
            helloCommand = new HelloCommand(monitor);
            monitor.RegisterCommand(helloCommand);
        }

        public void Dispose()
        {
            monitor.UnregisterCommand(helloCommand);
        }

        private readonly HelloCommand helloCommand;
        private readonly Monitor monitor;
    }
}
