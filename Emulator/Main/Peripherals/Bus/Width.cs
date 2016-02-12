//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Bus
{
    [Flags]
    public enum Width
    {
        Byte = 1,
        Word = 2,
        DoubleWord = 4,
        QuadWord = 8
    }
}

