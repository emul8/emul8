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
    public class WriteHookWrapper<T> : HookWrapper
    {
        public WriteHookWrapper(IBusPeripheral peripheral, Action<long, T> originalMethod, Func<T, long, T> newMethod = null,
                               Range? subrange = null) : base(peripheral, typeof(T), subrange)
        {
            this.originalMethod = originalMethod;
            this.newMethod = newMethod;
        }

        public Action<long, T> OriginalMethod
        {
            get
            {
                return originalMethod;
            }
        }

        public virtual void Write(long offset, T value)
        {
            if(Subrange != null && !Subrange.Value.Contains(offset))
            {
                originalMethod(offset, value);
                return;
            }
            var modifiedValue = newMethod(value, offset);
            originalMethod(offset, modifiedValue);
        }

        private readonly Action<long, T> originalMethod;
        private readonly Func<T, long, T> newMethod;
    }
}

