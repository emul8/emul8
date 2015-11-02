//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.Migrant;
using System;

namespace Emul8.Utilities.Binding
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ImportAttribute : TransientAttribute
    {
        public string Name { get; set; }
    }
}

