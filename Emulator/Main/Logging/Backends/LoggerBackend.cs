//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Emul8.Logging
{
    public abstract class LoggerBackend : ILoggerBackend
    {
        public virtual bool IsControllable { get { return true; } }

        public abstract void Log(LogEntry entry);

        public abstract void Dispose();

        public virtual void SetLogLevel(LogLevel level, int sourceId = -1)
        {
            if(sourceId != -1)
            {
                if(level == null)
                {
                    peripheralsWithDifferentLogging.Remove(sourceId);
                }
                else
                {
                    peripheralsWithDifferentLogging[sourceId] = level;
                }
            }
            else
            {
                logLevel = level;
            }
        }

        public LogLevel GetCustomLogLevel(int? id)
        {
            LogLevel result = null;
            if(id.HasValue)
            {
                peripheralsWithDifferentLogging.TryGetValue(id.Value, out result);
            }
            return result;
        }

        public IDictionary<int, LogLevel> GetCustomLogLevels()
        {
            return new ReadOnlyDictionary<int, LogLevel>(peripheralsWithDifferentLogging);
        }

        public LogLevel GetLogLevel()
        {
            return logLevel;
        }

        public void Reset()
        {
            logLevel = LogLevel.Info;
            peripheralsWithDifferentLogging.Clear();
        }

        protected bool ShouldBeLogged(LogEntry entry)
        {
            return entry.Type >= (GetCustomLogLevel(entry.SourceId) ?? logLevel);
        }

        protected LoggerBackend()
        {
            peripheralsWithDifferentLogging = new Dictionary<int, LogLevel>();
            logLevel = LogLevel.Debug;
        }

        protected LogLevel logLevel;
        private readonly Dictionary<int, LogLevel> peripheralsWithDifferentLogging;
    }
}

