//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;

namespace Emul8.Peripherals.Bus.Wrappers
{
    public abstract class HookWrapper
    {
        protected HookWrapper(IBusPeripheral peripheral, Type type, Range? subrange)
        {
            Peripheral = peripheral;
            Name = type.Name;
            Subrange = subrange;
        }

        protected readonly string Name;
        protected readonly IBusPeripheral Peripheral;
        protected readonly Range? Subrange;
    }
}

