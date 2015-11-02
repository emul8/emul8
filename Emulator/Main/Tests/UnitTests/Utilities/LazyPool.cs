//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.UnitTests.Utilities
{
    public sealed class LazyPool<T>
    {
        public LazyPool(Func<T> factory)
        {
            this.factory = factory;
            list = new List<T>();
        }

        public T this[int index]
        {
            get
            {
                CheckIndex(index);
                return list[index];
            }
            set
            {
                CheckIndex(index);
                list[index] = value;
            }
        }

        private void CheckIndex(int index)
        {
            while(index >= list.Count)
            {
                list.Add(factory());
            }
        }

        private readonly List<T> list;
        private readonly Func<T> factory;
    }
}
