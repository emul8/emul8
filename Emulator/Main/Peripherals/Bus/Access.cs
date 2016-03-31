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
    public enum Access
    {
        Read = 1,
        Write = 2,
        ReadAndWrite = 3
    }
}

