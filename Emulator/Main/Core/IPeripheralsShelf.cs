//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;
using System.Collections.Generic;

namespace Emul8.Core
{
    public interface IPeripheralsShelf
    {
        bool Contains(IPeripheral peripheral);
        void Remove(IPeripheral peripheral);
        IEnumerable<IPeripheral> GetAll();
        void Clear();
    }
}

