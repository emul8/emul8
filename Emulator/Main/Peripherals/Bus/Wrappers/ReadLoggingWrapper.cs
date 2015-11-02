//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;

namespace Emul8.Peripherals.Bus.Wrappers
{
    public class ReadLoggingWrapper<T> : ReadHookWrapper<T>
    {
        public ReadLoggingWrapper(IBusPeripheral peripheral, Func<long, T> originalMethod) :
            base(peripheral, originalMethod)
        {
            mapper = new RegisterMapper(peripheral.GetType());
        }

        public override T Read(long offset)
        {
            var originalValue = OriginalMethod(offset);
            Peripheral.DebugLog("Read{0} from 0x{1:X}{3}, returned 0x{2:X}.", Name, offset, originalValue, mapper.ToString(offset, " ({0})"));
            return originalValue;
        }

        private readonly RegisterMapper mapper;
    }
}

