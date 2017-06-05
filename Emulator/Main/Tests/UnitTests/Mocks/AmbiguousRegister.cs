//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;
using Emul8.Peripherals.Bus;

namespace Emul8.UnitTests.Mocks
{
    public class AmbiguousRegister : IPeripheralRegister<IDoubleWordPeripheral, BusPointRegistration>, IPeripheralRegister<IDoubleWordPeripheral, DoublePointRegistration>,
        IPeripheralRegister<IDoubleWordPeripheral, IMockRegistrationPoint1>, IPeripheralRegister<IDoubleWordPeripheral, IMockRegistrationPoint2>,
        IPeripheralRegister<IBytePeripheral, DoublePointRegistration>
    {
        public void Register(IDoubleWordPeripheral peripheral, DoublePointRegistration registrationPoint)
        {
            
        }

        public void Register(IDoubleWordPeripheral peripheral, BusPointRegistration registrationPoint)
        {
            
        }

        public void Register(IDoubleWordPeripheral peripheral, IMockRegistrationPoint1 registrationPoint)
        {
            
        }

        public void Register(IDoubleWordPeripheral peripheral, IMockRegistrationPoint2 registrationPoint)
        {
            
        }

        public void Register(IBytePeripheral peripheral, DoublePointRegistration registrationPoint)
        {
            
        }

        public void Unregister(IDoubleWordPeripheral peripheral)
        {
            
        }

        public void Unregister(IBytePeripheral peripheral)
        {
            
        }
    }

    public class DoublePointRegistration : IRegistrationPoint
    {
        public DoublePointRegistration(double from)
        {
            this.from = from;
        }

        public string PrettyString
        {
            get
            {
                return from.ToString();
            }
        }

        private readonly double from;
    }

    public class MockRegistrationPoint : IRegistrationPoint, IMockRegistrationPoint1, IMockRegistrationPoint2
    {
        public string PrettyString
        {
            get
            {
                return "";
            }
        }
    }

    public interface IMockRegistrationPoint1 : IRegistrationPoint
    {

    }

    public interface IMockRegistrationPoint2 : IRegistrationPoint
    {

    }
}
