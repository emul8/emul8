//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.CPU
{
    public class HaltArguments
    {
        public HaltArguments(HaltReason reason, long address = 0, BreakpointType? breakpointType = null)
        {
            Reason = reason;
            Address = address;
            BreakpointType = breakpointType;
        }

        public HaltReason Reason { get; private set; }
        public long Address { get; private set; }
        public BreakpointType? BreakpointType { get; private set; }
    }
}

