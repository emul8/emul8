//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Bus;
using Emul8.Logging;

namespace Emul8.Peripherals.Bus.Wrappers
{
    public sealed class WriteLoggingWrapper<T> : WriteHookWrapper<T>
    {
        public WriteLoggingWrapper(IBusPeripheral peripheral, Action<long, T> originalMethod) : base(peripheral, originalMethod, null, null)
        {
            mapper = new RegisterMapper(peripheral.GetType());
        }

        public override void Write(long offset, T value)
        {
            Peripheral.DebugLog("Write{0} to 0x{1:X}{3}, value 0x{2:X}.", Name, offset, value, mapper.ToString(offset, " ({0})"));
            OriginalMethod(offset, value);
        }

        private readonly RegisterMapper mapper;
    }
}

