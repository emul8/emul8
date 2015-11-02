//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;

namespace Emul8.Peripherals.Bus
{
    public class ConnectionRegionAttribute : Attribute
    {
        public ConnectionRegionAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

