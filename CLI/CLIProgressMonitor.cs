//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;
using Emul8.Logging;

namespace Emul8.CLI
{
    public class CLIProgressMonitor : IProgressMonitorHandler
    {
        #region IProgressMonitorHandler implementation

        public void Finish(int id)
        {
        }

        public void Update(int id, string description, int? progress)
        {
            var now = CustomDateTime.Now;
            if(now - lastUpdate > updateTime)
            {
                Logger.Log(LogLevel.Info, description);
                lastUpdate = now;
            }
        }

        #endregion

        public CLIProgressMonitor()
        {
            updateTime = TimeSpan.FromSeconds(3);
        }

        private TimeSpan updateTime;
        private DateTime lastUpdate;
    }
}

