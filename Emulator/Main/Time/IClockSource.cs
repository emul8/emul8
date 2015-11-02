//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Time
{
    public interface IClockSource
    {
        void ExecuteInLock(Action action);
        void AddClockEntry(ClockEntry entry);
        void ExchangeClockEntryWith(Action handler, Func<ClockEntry, ClockEntry> visitor,
            Func<ClockEntry> factoryIfNonExistant = null);
        bool RemoveClockEntry(Action handler);
        ClockEntry GetClockEntry(Action handler);
        void GetClockEntryInLockContext(Action handler, Action<ClockEntry> visitor);
        IEnumerable<ClockEntry> GetAllClockEntries();
        long CurrentValue { get; }
        IEnumerable<ClockEntry> EjectClockEntries();
        void AddClockEntries(IEnumerable<ClockEntry> entries);
    }
}

