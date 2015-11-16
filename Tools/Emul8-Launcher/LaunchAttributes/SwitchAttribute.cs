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
    public class SwitchAttribute : Attribute
    {
        public SwitchAttribute(string longSwitch, char shortSwitch) : this(longSwitch)
        {
            ShortSwitch = shortSwitch;
        }

        public SwitchAttribute(string longSwitch)
        {
            LongSwitch = longSwitch;
        }

        public char? ShortSwitch { get; private set; }
        public string LongSwitch { get; private set; }
    }
}
