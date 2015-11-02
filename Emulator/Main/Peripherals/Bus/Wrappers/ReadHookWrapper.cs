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
    public class ReadHookWrapper<T> : HookWrapper
    {
        public ReadHookWrapper(IBusPeripheral peripheral, Func<long, T> originalMethod, Func<T, long, T> newMethod = null,
                              Range? subrange = null) : base(peripheral, typeof(T), subrange)
        {
            this.originalMethod = originalMethod;
            this.newMethod = newMethod;
        }

        public Func<long, T> OriginalMethod
        {
            get
            {
                return originalMethod;
            }
        }

        public virtual T Read(long offset)
        {
            if(Subrange != null && !Subrange.Value.Contains(offset))
            {
                return originalMethod(offset);
            }
            return newMethod(originalMethod(offset), offset);
        }

        private readonly Func<long, T> originalMethod;
        private readonly Func<T, long, T> newMethod;
    }
}

