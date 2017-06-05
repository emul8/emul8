//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Utilities.Collections
{
    public sealed class LazyList<T>
    {
        public LazyList()
        {
            funcs = new List<Func<T>>();
        }

        public void Add(Func<T> func)
        {
            funcs.Add(func);
        }

        public List<T> ToList()
        {
            return funcs.Select(x => x()).ToList();
        }

        private readonly List<Func<T>> funcs;
    }
}
