//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;

namespace Emul8.Core.Structure
{
    /// <summary>
    /// An object that allows registration of TPeripheral using TRegistrationPoint.
    /// NOTE: This exists along IPeripheralContainer because some objects handle more than
    /// one TRegistrationPoint for a given TPeripheral.
    /// </summary>
    public interface IPeripheralRegister<TPeripheral, TRegistrationPoint> : ICovariantPeripheralRegister<TPeripheral, TRegistrationPoint>
        where TPeripheral : IPeripheral where TRegistrationPoint : IRegistrationPoint
    {
        void Register(TPeripheral peripheral, TRegistrationPoint registrationPoint);
        void Unregister(TPeripheral peripheral);
    }

    // this interface is needed for `IRegisterController` which describes controller of 'any' register
    // that is encoded as IPeripheralRegister<IPerhipheral, IRegistrationPoint> (that's why we need out)
    public interface ICovariantPeripheralRegister<out TPeripheral, out TRegistrationPoint> : IEmulationElement
        where TPeripheral : IPeripheral where TRegistrationPoint : IRegistrationPoint
    {
    }
}

