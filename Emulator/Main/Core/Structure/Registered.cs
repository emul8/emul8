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
    public class Registered<TPeripheral, TRegistrationPoint> : IRegistered<TPeripheral, TRegistrationPoint>
        where TPeripheral : IPeripheral where TRegistrationPoint : IRegistrationPoint
    {
        public Registered(TPeripheral peripheral, TRegistrationPoint registrationPoint)
        {
            Peripheral = peripheral;
            RegistrationPoint = registrationPoint;
        }

        public TPeripheral Peripheral { get; private set; }
        public TRegistrationPoint RegistrationPoint { get; private set; }
    }

    public static class Registered
    {
        public static Registered<TPeripheral, TRegistrationPoint> Create<TPeripheral, TRegistrationPoint>
            (TPeripheral peripheral, TRegistrationPoint registrationPoint)
            where TPeripheral : IPeripheral  where TRegistrationPoint : IRegistrationPoint
        {
            return new Registered<TPeripheral, TRegistrationPoint>(peripheral, registrationPoint);
        }
    }
}
