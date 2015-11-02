//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;

namespace Emul8.Utilities.Collections
{
    public class FastReadConcurrentCollection<T>
    {
        public FastReadConcurrentCollection()
        {
            Items = new T[0];
        }

        public void Add(T item)
        {
            lock(locker)
            {
                var copy = new List<T>(Items);
                copy.Add(item);
                Items = copy.ToArray();
            }
        }

        public void Remove(T item)
        {
            lock(locker)
            {
                var copy = new List<T>(Items);
                copy.Remove(item);
                Items = copy.ToArray();
            }
        }

        public T[] Items { get; private set; }

        private readonly object locker = new object();
    }
}

