//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB
{
    internal static class BreakpointTypeExtensions
    {
        public static string GetStopReason(this BreakpointType type)
        {
            switch (type) 
            {
            case BreakpointType.AccessWatchpoint:
                return "awatch";
            case BreakpointType.WriteWatchpoint:
                return "watch";
            case BreakpointType.ReadWatchpoint:
                return "rwatch";
            case BreakpointType.HardwareBreakpoint:
                return "hwbreak";
            case BreakpointType.MemoryBreakpoint:
                return "swbreak";
            default:
                throw new ArgumentException();
            }
        }
    }
}

