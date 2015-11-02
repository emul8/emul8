//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using Emul8.Peripherals;

namespace Emul8.Core.Structure
{
    /// <summary>
    /// Interface for objects that allow registering peripherals and addressing/querying for them.
    /// </summary>
    public interface IPeripheralContainer<TPeripheral, TRegistrationPoint> :
        IPeripheralRegister<TPeripheral, TRegistrationPoint>
        where TPeripheral : IPeripheral where TRegistrationPoint : IRegistrationPoint
    {
        IEnumerable<TRegistrationPoint> GetRegistrationPoints(TPeripheral peripheral);
        IEnumerable<IRegistered<TPeripheral, TRegistrationPoint>> Children { get; }
    }
}
