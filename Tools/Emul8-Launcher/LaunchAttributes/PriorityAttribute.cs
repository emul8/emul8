//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.LaunchAttributes
{
    public class PriorityAttribute : Attribute
    {
        public PriorityAttribute(uint priority)
        {
            Priority = priority;
        }

        public uint Priority { get; private set; }
    }
}
