//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Peripherals.CPU.Registers
{
    public interface IRegisters
    {
        IEnumerable<int> Keys { get; }
    }

    public interface IRegisters<T> : IRegisters
    {
        T this[int index] { get; set; }
    }
}

