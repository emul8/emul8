//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Input
{
    [Flags]
    public enum MouseButton
    {
        Left   = 0x01,
        Right  = 0x02,
        Middle = 0x04,
        Side   = 0x08,
        Extra  = 0x10
    }
}

