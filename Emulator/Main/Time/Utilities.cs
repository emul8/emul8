//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;
using Emul8.Logging;
using Emul8.Utilities;

namespace Emul8.Time
{
    public static class Utilities
    {
        public static long SecondsToTicks(double seconds)
        {
            return (int)Math.Round(seconds * Consts.TicksPerSecond);
        }

        public static long TicksToSeconds(long ticks)
        {
            return (long)TimeSpan.FromTicks(ticks * Consts.TimeQuantum.Ticks).TotalSeconds;
        }

        public static long TicksToHz(long ticks)
        {
            return (long)Math.Round(1 / TimeSpan.FromTicks(Consts.TimeQuantum.Ticks * ticks).TotalSeconds);
        }

        public static double HzToTicks(long hz)
        {
            return 1.0*TimeSpan.FromSeconds(1.0).Ticks / hz / Consts.TimeQuantum.Ticks;
        }

        public static long HzToTicksSafe(long hz, IEmulationElement sender)
        {
            var result = (long)Math.Round(HzToTicks(hz));
            if(result == 0)
            {
                result = 1;
            }
            var againInHz = TicksToHz(result);
            if(Math.Abs(againInHz - hz) / hz > MaximalDifference)
            {
                sender.Log(LogLevel.Warning, "Significant difference between desired and actual frequency: {0}Hz={1}Hz vs {2}Hz={3}Hz,", 
                           Misc.NormalizeDecimal(hz), Misc.NormalizeDecimal(againInHz), hz, againInHz);
            }
            return result;
        }

        public static bool HzToTicks(long hz, out long ticks)
        {
            ticks = (long)Math.Round(HzToTicks(hz));
            return ticks != 0;
        }

        public static long MaxHz
        {
            get
            {
                return TicksToHz(1);
            }
        }

        private const double MaximalDifference = 0.33;
    }
}

