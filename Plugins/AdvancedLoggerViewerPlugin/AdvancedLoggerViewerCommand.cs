//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using Emul8.UserInterface.Commands;
using Emul8.UserInterface;
using Xwt;
using Emul8.Logging.Backends;
using Emul8.CLI;

namespace Emul8.Plugins.AdvancedLoggerViewer
{
    public class AdvancedLoggerViewerCommand : Emul8.UserInterface.Commands.Command
    {
        public AdvancedLoggerViewerCommand(Monitor monitor) : base(monitor, "showLogger", "Advanced logger viewer")
        {
        }

        [Runnable]
        public void Run()
        {
            ApplicationExtensions.InvokeInUIThread(() => {
                var window = new Window();
                window.Width = 800;
                window.Height = 600;
                window.Content = new LogViewer(LuceneLoggerBackend.Instance);
                window.Show();
            });
        }
    }
}

