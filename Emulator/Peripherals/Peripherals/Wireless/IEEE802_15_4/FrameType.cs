//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Peripherals.Wireless.IEEE802_15_4
{
    public enum FrameType : byte
    {
        Beacon = 0x0,
        Data = 0x1,
        ACK = 0x2,
        MACControl = 0x3,
        Reserved4 = 0x4,
        Reserved5 = 0x5,
        Reserved6 = 0x6,
        Reserved7 = 0x7
    }
}

