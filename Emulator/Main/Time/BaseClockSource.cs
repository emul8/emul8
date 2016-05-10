//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Antmicro.Migrant;
using Emul8.Logging;

namespace Emul8.Time
{
    public class BaseClockSource : IClockSource
    {
        public BaseClockSource()
        {
            clockEntries = new List<ClockEntry>();
            clockEntriesUpdateHandlers = new List<UpdateHandlerDelegate>();
            toNotify = new List<Action>();
            nearestTickIn = long.MaxValue;
            sync = new object();
            updateAlreadyInProgress = new ThreadLocal<bool>();
        }

        public long NearestTickIn
        {
            get
            {
                lock(sync)
                {
                    return nearestTickIn;
                }
            }
        }

        public void Advance(long ticks, bool immediately = false)
        {
            #if DEBUG
            if(ticks < 0)
            {
                throw new ArgumentException("Ticks cannot be negative.");
            }
            #endif
            lock(sync)
            {
                if(ticks > nearestTickIn)
                {
                    var left = ticks - nearestTickIn;
                    AdvanceInner(nearestTickIn, immediately);
                    Advance(left, immediately);
                }
                else
                {
                    AdvanceInner(ticks, immediately);
                }
            }
        }

        public void AdvanceInner(long ticks, bool immediately)
        {
            #if DEBUG
            if(ticks > nearestTickIn)
            {
                throw new InvalidOperationException("Should not reach here.");
            }
            #endif
            lock(sync)
            {
                elapsed += ticks;
                totalElapsed += ticks;
                if(nearestTickIn > ticks && !immediately)
                {
                    // nothing happens
                    nearestTickIn -= ticks;
                    return;
                }
                Update(elapsed);
                elapsed = 0;
            }
        }

        public virtual void ExecuteInLock(Action action)
        {
            lock(sync)
            {
                action();
            }
        }

        public virtual void AddClockEntry(ClockEntry entry)
        {
            lock(sync)
            {
                if(clockEntries.FindIndex(x => x.Handler == entry.Handler) != -1)
                {
                    throw new ArgumentException("A clock entry with given handler already exists in the clock source.");
                }
                clockEntries.Add(entry);
                clockEntriesUpdateHandlers.Add(null);
                UpdateUpdateHandler(clockEntries.Count - 1);
                UpdateLimits();
            }
            NotifyNumberOfEntriesChanged(clockEntries.Count - 1, clockEntries.Count);
        }

        public virtual void ExchangeClockEntryWith(Action handler, Func<ClockEntry, ClockEntry> visitor,
            Func<ClockEntry> factoryIfNonExistent)
        {
            lock(sync)
            {
                UpdateLimits();
                var indexOfEntry = clockEntries.FindIndex(x => x.Handler == handler);

                if(indexOfEntry == -1)
                {
                    if(factoryIfNonExistent != null)
                    {
                        clockEntries.Add(factoryIfNonExistent());
                        clockEntriesUpdateHandlers.Add(null);
                        UpdateUpdateHandler(clockEntries.Count - 1);
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }
                else
                {
                    clockEntries[indexOfEntry] = visitor(clockEntries[indexOfEntry]);
                    UpdateUpdateHandler(indexOfEntry);
                }
                UpdateLimits();
            }
        }

        public virtual ClockEntry GetClockEntry(Action handler)
        {
            lock(sync)
            {
                UpdateLimits();
                var result = clockEntries.FirstOrDefault(x => x.Handler == handler);
                if(result.Handler == null)
                {
                    throw new KeyNotFoundException();
                }
                return result;
            }
        }

        public virtual void GetClockEntryInLockContext(Action handler, Action<ClockEntry> visitor)
        {
            lock(sync)
            {
                UpdateLimits();
                var result = clockEntries.FirstOrDefault(x => x.Handler == handler);
                if(result.Handler == null)
                {
                    throw new KeyNotFoundException();
                }
                visitor(result);
            }
        }

        public IEnumerable<ClockEntry> GetAllClockEntries()
        {
            lock(sync)
            {
                return clockEntries.ToList();
            }
        }

        public virtual bool RemoveClockEntry(Action handler)
        {
            int oldCount;
            lock(sync)
            {
                oldCount = clockEntries.Count;
                var indexToRemove = clockEntries.FindIndex(x => x.Handler == handler);
                if(indexToRemove == -1)
                {
                    return false;
                }
                clockEntries.RemoveAt(indexToRemove);
                clockEntriesUpdateHandlers.RemoveAt(indexToRemove);
                UpdateLimits();
            }
            NotifyNumberOfEntriesChanged(oldCount, clockEntries.Count);
            return true;
        }

        public virtual long CurrentValue
        {
            get
            {
                return totalElapsed;
            }
        }

        public virtual IEnumerable<ClockEntry> EjectClockEntries()
        {
            int oldCount;
            IEnumerable<ClockEntry> result;
            lock(sync)
            {
                oldCount = clockEntries.Count;
                result = clockEntries.ToArray();
                clockEntries.Clear();
                clockEntriesUpdateHandlers.Clear();
            }
            NotifyNumberOfEntriesChanged(oldCount, 0);
            return result;
        }

        public void AddClockEntries(IEnumerable<ClockEntry> entries)
        {
            lock(sync)
            {
                foreach(var entry in entries)
                {
                    AddClockEntry(entry);
                }
            }
        }

        public bool HasEntries
        {
            get
            {
                lock(sync)
                {
                    return clockEntries.Count > 0;
                }
            }
        }

        public event Action<int, int> NumberOfEntriesChanged;

        private static bool HandleDirectionDescendingPositiveRatio(ref ClockEntry entry, long ticks, ref long nearestTickIn) 
        {
            var flag = false;

            entry.Value -= ticks * entry.Ratio;
            entry.ValueResiduum = 0;
            if(entry.Value <= 0)
            {
                flag = true;
                entry.Value = entry.Period;
                entry = entry.With(enabled: entry.Enabled & (entry.WorkMode != WorkMode.OneShot));
            }

            nearestTickIn = Math.Min(nearestTickIn, (entry.Value - 1) / entry.Ratio + 1);
            return flag;
        }

        private static bool HandleDirectionDescendingNegativeRatio(ref ClockEntry entry, long ticks, ref long nearestTickIn) 
        {
            var flag = false;
                        
            entry.Value -= (ticks + entry.ValueResiduum) / -entry.Ratio;
            entry.ValueResiduum = (ticks + entry.ValueResiduum) % -entry.Ratio;
            if(entry.Value <= 0)
            {
                // TODO: maybe issue warning if its lower than zero
                flag = true;
                entry.Value = entry.Period;
                entry = entry.With(enabled: entry.Enabled & (entry.WorkMode != WorkMode.OneShot));
            }

            nearestTickIn = Math.Min(nearestTickIn, entry.Value * -entry.Ratio + entry.ValueResiduum);
            return flag;
        }

        private static bool HandleDirectionAscendingPositiveRatio(ref ClockEntry entry, long ticks, ref long nearestTickIn) 
        {
            var flag = false;
                    
            entry.Value += ticks * entry.Ratio;
            entry.ValueResiduum = 0;
            
            if(entry.Value >= entry.Period)
            {
                flag = true;
                entry.Value = 0;
                entry = entry.With(enabled: entry.Enabled & (entry.WorkMode != WorkMode.OneShot));
            }

            nearestTickIn = Math.Min(nearestTickIn, (entry.Period - entry.Value - 1) / entry.Ratio + 1);
            return flag;
        }

        private static bool HandleDirectionAscendingNegativeRatio(ref ClockEntry entry, long ticks, ref long nearestTickIn) 
        {
            var flag = false;
                        
            entry.Value += (ticks + entry.ValueResiduum) / -entry.Ratio;
            entry.ValueResiduum = (ticks + entry.ValueResiduum) % -entry.Ratio;

            if(entry.Value >= entry.Period)
            {
                flag = true;
                entry.Value = 0;
                entry = entry.With(enabled: entry.Enabled & (entry.WorkMode != WorkMode.OneShot));
            }

            nearestTickIn = Math.Min(nearestTickIn, ((entry.Period - entry.Value) * -entry.Ratio) - entry.ValueResiduum);
            return flag;
        }

        private void NotifyNumberOfEntriesChanged(int oldValue, int newValue)
        {
            var numberOfEntriesChanged = NumberOfEntriesChanged;
            if(numberOfEntriesChanged != null)
            {
                numberOfEntriesChanged(oldValue, newValue);
            }
        }

        private void UpdateLimits()
        {
            AdvanceInner(0, true);
        }

        private void Update(long ticks)
        {
            if(updateAlreadyInProgress.Value)
            {
                return;
            }
            try
            {
                updateAlreadyInProgress.Value = true;
                lock(sync)
                {
                    nearestTickIn = long.MaxValue;
                    for(var i = 0; i < clockEntries.Count; i++)
                    {
                        var clockEntry = clockEntries[i];
                        var updateHandler = clockEntriesUpdateHandlers[i];
                        if(!clockEntry.Enabled)
                        {
                            continue;
                        }
                        if(updateHandler(ref clockEntry, ticks, ref nearestTickIn))
                        {
                            toNotify.Add(clockEntry.Handler);
                        }
                        clockEntries[i] = clockEntry;
                    }
                }
                foreach(var action in toNotify)
                {
                    action();
                }
                toNotify.Clear();
            }
            finally
            {
                updateAlreadyInProgress.Value = false;
            }
        }

        private void UpdateUpdateHandler(int clockEntryIndex)
        {
            if(clockEntries[clockEntryIndex].Direction == Direction.Descending)
            {
                if(clockEntries[clockEntryIndex].Ratio > 0)
                {
                    clockEntriesUpdateHandlers[clockEntryIndex] = HandleDirectionDescendingPositiveRatio;
                }
                else
                {
                    clockEntriesUpdateHandlers[clockEntryIndex] = HandleDirectionDescendingNegativeRatio;
                }
            }
            else
            {
                if(clockEntries[clockEntryIndex].Ratio > 0)
                {
                    clockEntriesUpdateHandlers[clockEntryIndex] = HandleDirectionAscendingPositiveRatio;
                }
                else
                {
                    clockEntriesUpdateHandlers[clockEntryIndex] = HandleDirectionAscendingNegativeRatio;
                }
            }
        }

        [Constructor]
        private ThreadLocal<bool> updateAlreadyInProgress;

        private long nearestTickIn;
        private long elapsed;
        private long totalElapsed;
        private readonly List<Action> toNotify;
        private readonly List<ClockEntry> clockEntries;
        private readonly List<UpdateHandlerDelegate> clockEntriesUpdateHandlers;
        private readonly object sync;

        private delegate bool UpdateHandlerDelegate(ref ClockEntry entry, long ticks, ref long nearestTickIn);
    }
}

