//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Bus
{
    [Flags]
    public enum AllowedTranslation
    {
        ByteToWord = 1 << 0,
        ByteToDoubleWord = 1 << 1,
        WordToByte = 1 << 2,
        WordToDoubleWord = 1 << 3,
        DoubleWordToByte = 1 << 4,
        DoubleWordToWord = 1 << 5,
    }
}

