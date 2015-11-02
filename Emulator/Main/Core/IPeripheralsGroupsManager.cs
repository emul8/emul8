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
    public interface IPeripheralsGroupsManager
    {
        IEnumerable<IPeripheralsGroup> ActiveGroups { get; }
        bool TryGetByName(string name, out IPeripheralsGroup group);
        IPeripheralsGroup GetOrCreate(string name, IEnumerable<IPeripheral> peripherals);
        bool TryGetActiveGroupContaining(IPeripheral peripheral, out IPeripheralsGroup group);
        bool TryGetAnyGroupContaining(IPeripheral peripheral, out IPeripheralsGroup group);
    }
}

