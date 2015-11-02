//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using IronPython.Runtime;

namespace Emul8.Peripherals.Python
{
    public class PythonDictionarySurrogate : Dictionary<object, object>
    {
        public PythonDictionarySurrogate(PythonDictionary dictionary)
        {
            internalDictionary = new Dictionary<object, object>(dictionary);
        }

        public PythonDictionary Restore()
        {
            var pythonDictionary = new PythonDictionary();
            foreach(var item in internalDictionary)
            {
                pythonDictionary.Add(item);
            }
            return pythonDictionary;
        }

        private readonly Dictionary<object, object> internalDictionary;
    }
}

