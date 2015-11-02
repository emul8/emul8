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
      public enum MessageRecipient
        {
            Device = 0,
            Interface = 1,
            Endpoint = 2,
            Other = 3
        }
   public struct USBPacket
        {
            public byte ep;
            public byte [] data;
            public long bytesToTransfer;
        }
}
