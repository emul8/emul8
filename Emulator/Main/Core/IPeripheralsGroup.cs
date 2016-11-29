//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using Emul8.Peripherals;

namespace Emul8.Core
{
    public interface IPeripheralsGroup
    {
        string Name { get; } 
        Machine Machine { get; }
        IEnumerable<IPeripheral> Peripherals { get; }

        void Unregister();
    }
}

