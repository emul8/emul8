//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;

namespace Emul8.Core.Structure.Registers
{
    /// <summary>
    /// Information about an unhandled field in a register.
    /// </summary>
    public struct Tag
    {
        public String Name;
        public int Position;
        public int Width;
    }
}

