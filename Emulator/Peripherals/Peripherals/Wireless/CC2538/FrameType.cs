//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Peripherals.Wireless.CC2538
{
    internal enum FrameType : byte
    {
        Beacon = 0x0,
        Data = 0x1,
        ACK = 0x2,
        MACControl = 0x3
    }
}

