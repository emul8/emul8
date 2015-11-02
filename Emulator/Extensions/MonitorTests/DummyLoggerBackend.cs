//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Logging;
using System.Collections.Generic;

namespace MonitorTests
{
    public class DummyLoggerBackend : ILoggerBackend
    {
        public bool IsControllable { get { return true; } }
        public bool AcceptEverything { get { return false; } }

        public IDictionary<int, LogLevel> GetCustomLogLevels()
        {
            return new Dictionary<int, LogLevel>();
        }

        public LogEntry NextEntry()
        {
            if(entries.Count > 0)
            {
                return entries.Dequeue();
            }
            return null;
        }

        public LogLevel GetLogLevel()
        {
            return LogLevel.Noisy;
        }

        public void Log(LogEntry entry)
        {
            entries.Enqueue(entry);
        }

        public void Clear()
        {
            entries.Clear();
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }

        public void SetLogLevel(LogLevel level, int sourceId = -1)
        {
        }

        public bool ShouldBePrinted(object obj, LogLevel level)
        {
            return true;
        }

        private readonly Queue<LogEntry> entries = new Queue<LogEntry>();
    }
}

