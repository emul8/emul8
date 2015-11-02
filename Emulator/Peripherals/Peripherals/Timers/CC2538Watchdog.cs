//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;

namespace Emul8.Peripherals.Timers
{
    public sealed class CC2538Watchdog : SimpleTicker, IKnownSize
    {
        public CC2538Watchdog(long periodInMs, Machine machine) : base(periodInMs, machine)
        {
            Reset();
        }

        public override void WriteDoubleWord(long offset, uint value)
        {
            if(value == 0x5 && previousValue == 0xA)
            {
                Reset();
            }
            else
            {
                previousValue = value;
            }
        }

        public override void Reset()
        {
            previousValue = 0;
            base.Reset();
        }

        public long Size
        {
            get
            {
                return 0x4;
            }
        }

        private uint previousValue;
    }
}

