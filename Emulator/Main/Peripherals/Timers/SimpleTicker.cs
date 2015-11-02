//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Time;
using Emul8.Logging;
using System.Threading;

namespace Emul8.Peripherals.Timers
{
    public class SimpleTicker : IDoubleWordPeripheral
    {
        public SimpleTicker(long periodInMs, Machine machine)
        {
            var clockSource = machine.ObtainClockSource();
            clockSource.AddClockEntry(new ClockEntry(periodInMs, ClockEntry.FrequencyToRatio(this, 1000), OnTick));
        }

        public virtual uint ReadDoubleWord(long offset)
        {
            return (uint)Interlocked.CompareExchange(ref counter, 0, 0);
        }

        public virtual void WriteDoubleWord(long offset, uint value)
        {
            this.LogUnhandledWrite(offset, value);
        }

        public virtual void Reset()
        {
            Interlocked.Exchange(ref counter, 0);
        }

        private void OnTick()
        {
            Interlocked.Increment(ref counter);
        }

        private int counter;
    }
}

