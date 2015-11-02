//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.USB
{
   public struct USBSetupPacket
        {
            public byte requestType;
            public byte request;
            public UInt16 value;
            public UInt16 index;
            public UInt16 length;
        }
}
