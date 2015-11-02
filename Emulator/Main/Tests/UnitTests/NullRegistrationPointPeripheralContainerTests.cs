//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using NUnit.Framework;
using Emul8.Peripherals;
using Emul8.Core.Structure;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using System.Linq;
using Emul8.Exceptions;

namespace Emul8.UnitTests
{
    [TestFixture]
    public class NullRegistrationPointPeripheralContainerTests
    {   
        [Test]
        public void ShouldRegisterPeripheral()
        {
            container.Register(peripheral, registrationPoint);
            Assert.IsTrue(machine.IsRegistered(peripheral));
        }

        [Test]
        [ExpectedException(typeof(RegistrationException))]
        public void ShouldThrowWhenSecondPeripheral()
        {
            container.Register(peripheral2, NullRegistrationPoint.Instance);
            container.Register(peripheral, NullRegistrationPoint.Instance);
        }

        [Test]
        public void ShouldUnregisterPeripheral()
        {
            container.Register(peripheral, registrationPoint);
            container.Unregister(peripheral);
            Assert.IsFalse(machine.IsRegistered(peripheral));
        }

        [Test]
        [ExpectedException(typeof(RegistrationException))]
        public void ShouldThrowWhenUnregisteringNotRegisteredPeripheral()
        {
            container.Register(peripheral2, NullRegistrationPoint.Instance);
            container.Unregister(peripheral);
        }

        [Test]
        [ExpectedException(typeof(RegistrationException))]
        public void ShouldThrowWhenUnregisteringFromEmptyContainers()
        {
            container.Unregister(peripheral);
        }

        [Test]
        public void ShouldGetRegistrationPoints()
        {
            container.Register(peripheral, registrationPoint);
            Assert.AreEqual(1, container.GetRegistrationPoints(peripheral).Count());
            Assert.AreSame(NullRegistrationPoint.Instance, container.GetRegistrationPoints(peripheral).First());
        }

        [Test]
        public void ShouldGetEmptyRegistrationPoints()
        {
            Assert.IsEmpty(container.GetRegistrationPoints(peripheral));
            container.Register(peripheral, registrationPoint);
            container.Unregister(peripheral);
            Assert.IsEmpty(container.GetRegistrationPoints(peripheral));
        }

        [Test]
        public void ShouldGetRegisteredPeripheralAsChildren()
        {
            container.Register(peripheral, registrationPoint);
            Assert.AreEqual(1, container.GetRegistrationPoints(peripheral).Count());
            Assert.AreSame(peripheral, container.Children.First().Peripheral);
            Assert.AreSame(NullRegistrationPoint.Instance, container.Children.First().RegistrationPoint);
        }

        [Test]
        public void ShouldGetEmptyChildren()
        {
            Assert.IsEmpty(container.Children);
            container.Register(peripheral, registrationPoint);
            container.Unregister(peripheral);
            Assert.IsEmpty(container.Children);
        }

        [Test]
        public void ShouldRegister2ndAfterUnregistering()
        {
            container.Register(peripheral, registrationPoint);
            container.Unregister(peripheral);
            container.Register(peripheral2, registrationPoint);
            Assert.IsFalse(machine.IsRegistered(peripheral));
            Assert.IsTrue(machine.IsRegistered(peripheral2));
        }


        [SetUp]
        public void SetUp()
        {
            var sysbusRegistrationPoint = new BusRangeRegistration(1337, 666);
            machine = new Machine();
            peripheral = new PeripheralMock();
            peripheral2 = new PeripheralMock();
            container = new NullRegistrationPointPeripheralContainerMock(machine);
            registrationPoint = NullRegistrationPoint.Instance;
            machine.SystemBus.Register(container, sysbusRegistrationPoint);
        }

        private Machine machine;
        private PeripheralMock peripheral;
        private PeripheralMock peripheral2;
        private NullRegistrationPointPeripheralContainerMock container;
        private NullRegistrationPoint registrationPoint;

        private class NullRegistrationPointPeripheralContainerMock : 
            NullRegistrationPointPeripheralContainer<PeripheralMock>,
        IDoubleWordPeripheral
        {
            public NullRegistrationPointPeripheralContainerMock(Machine machine) : base(machine) {}
            public override void Reset(){}
            public void WriteDoubleWord(long offset, uint value){}
            public uint ReadDoubleWord(long offset)
            {
                return 1337;
            }
        }

        private class PeripheralMock : IPeripheral
        {
            public void Reset(){}
        }
    }
}
