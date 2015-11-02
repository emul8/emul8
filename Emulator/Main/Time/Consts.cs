//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Time
{
    public static class Consts
    {
        public static readonly TimeSpan TimeQuantum = TimeSpan.FromTicks(10);

        public static readonly long TicksPerSecond = TimeSpan.TicksPerSecond / TimeQuantum.Ticks;
    }
}

