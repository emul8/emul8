//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Logging
{
    public interface ILoggerBackend : IDisposable
    {
        void Log(LogEntry entry);
        void SetLogLevel(LogLevel level, int sourceId = -1);
        LogLevel GetLogLevel();
        IDictionary<int, LogLevel> GetCustomLogLevels();
        void Reset();

        bool IsControllable { get; }
    }
}
