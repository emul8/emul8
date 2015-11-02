//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.UserInterface.Tokenizer;

namespace Emul8.UserInterface.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RunnableAttribute:Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValuesAttribute:Attribute
    {
        public IEnumerable<object> Values { get; set; }

        public ValuesAttribute(params object[] values)
        {
            Values = new List<object>(values);
        }
    }
}

