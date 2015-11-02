//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using NUnit.Framework;
using Emul8.Core;
using Moq;
using Emul8.Peripherals;
using Emul8.Peripherals.Bus;
using Emul8.Core.Structure;
using System.Threading;
using Emul8.Exceptions;

namespace Emul8.UnitTests
{
    public class MachineTests
    {
        [Test]
        public void ShouldPutOnShelfUnregisteredPeripheral()
        {
            var machine = new Machine();
            var peripheral = new Mock<IPeripheral>();
            machine.PutOnShelf(peripheral.Object, "something");

            Assert.IsTrue(machine.Shelf.Contains(peripheral.Object));
        }

        [Test]
        public void ShouldPutOnShelfRegisteredPeripheral()
        {
            var machine = new Machine();
            var peripheral = new Mock<IDoubleWordPeripheral>();
            machine.SystemBus.Register(peripheral.Object, new BusRangeRegistration(0.To(10)));
            machine.SetLocalName(peripheral.Object, "something");
            machine.PutOnShelf(peripheral.Object);

            Assert.IsFalse(machine.IsRegistered(peripheral.Object));
            Assert.IsTrue(machine.Shelf.Contains(peripheral.Object));
        }

        [Test]
        public void ShouldPutPeripheralsTreeOnShelf()
        {
            var machine = new Machine();
            var mother = new Mother(machine);
            machine.SystemBus.Register(mother, 0.To(10));
            machine.SetLocalName(mother, "mother");
            var childMock = new Mock<IPeripheral>();
            var child = childMock.Object;
            mother.Register(child, NullRegistrationPoint.Instance);
            machine.SetLocalName(child, "child");
            machine.PutOnShelf(mother);

            Assert.IsFalse(machine.IsRegistered(mother));
            Assert.IsFalse(machine.IsRegistered(child));
            Assert.IsTrue(machine.Shelf.Contains(mother));
            Assert.IsTrue(machine.Shelf.Contains(child));
        }

        [Test]
        public void ShouldHandleManagedThreadsOnShelf()
        {
            var machine = new Machine();
            var peripheral = new PeripheralWithManagedThread(machine);
            machine.SystemBus.Register(peripheral, 0.To(10));
            machine.SetLocalName(peripheral, "test");

            machine.Start();
            Assert.IsTrue(peripheral.IsTheThreadRunning);
            machine.Pause();
            Assert.IsFalse(peripheral.IsTheThreadRunning);

            machine.Start();
            Assert.IsTrue(peripheral.IsTheThreadRunning);
            machine.PutOnShelf(peripheral);
            Assert.IsFalse(peripheral.IsTheThreadRunning);
            machine.SystemBus.Register(peripheral, 0.To(10));
            Assert.IsTrue(peripheral.IsTheThreadRunning);
        }

        [Test]
        public void ShouldThrowOnRegisteringAnotherPeripheralWithTheSameName()
        {
            var machine = new Machine();
            var peripheral1 = new Mock<IDoubleWordPeripheral>().Object;
            var peripheral2 = new Mock<IDoubleWordPeripheral>().Object;
            machine.SystemBus.Register(peripheral1, 0.To(10));
            machine.SystemBus.Register(peripheral2, 10.To(20));
            machine.SetLocalName(peripheral1, "name");

            Assert.Throws(typeof(RecoverableException), () => machine.SetLocalName(peripheral2, "name"));
        }

        [Test]
        public void ShouldFindPeripheralByPath()
        {
            var machine = new Machine();
            var peripheral1 = new Mock<IDoubleWordPeripheral>().Object;
            machine.SystemBus.Register(peripheral1, 0.To(10));
            machine.SetLocalName(peripheral1, "name");

            Assert.AreEqual(peripheral1, machine["sysbus.name"]);
        }

        [Test]
        public void ShouldFindPeripheralByPathWhenThereAreTwo()
        {
            var machine = new Machine();
            var peripheral1 = new Mock<IDoubleWordPeripheral>().Object;
            var peripheral2 = new Mock<IDoubleWordPeripheral>().Object;
            machine.SystemBus.Register(peripheral1, 0.To(10));
            machine.SystemBus.Register(peripheral2, 10.To(20));
            machine.SetLocalName(peripheral1, "first");
            machine.SetLocalName(peripheral2, "second");

            Assert.AreEqual(peripheral1, machine["sysbus.first"]);
            Assert.AreEqual(peripheral2, machine["sysbus.second"]);
        }

        [Test]
        public void ShouldThrowOnNullOrEmptyPeripheralName()
        {
            var machine = new Machine();
            var peripheral1 = new Mock<IDoubleWordPeripheral>().Object;
            machine.SystemBus.Register(peripheral1, 0.To(10));

            Assert.Throws(typeof(RecoverableException), () => machine.SetLocalName(peripheral1, ""));
            Assert.Throws(typeof(RecoverableException), () => machine.SetLocalName(peripheral1, null));
        }

        public sealed class Mother : IPeripheralRegister<IPeripheral, NullRegistrationPoint>, IDoubleWordPeripheral
        {
            public Mother(Machine machine)
            {
                this.machine = machine;
            }

            public void Register(IPeripheral peripheral, NullRegistrationPoint registrationPoint)
            {
                machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
            }

            public void Unregister(IPeripheral peripheral)
            {
                machine.UnregisterAsAChildOf(this, peripheral);
            }
                
            public uint ReadDoubleWord(long offset)
            {
                return 0;
            }

            public void WriteDoubleWord(long offset, uint value)
            {

            }

            public void Reset()
            {

            }

            private readonly Machine machine;
        }

        public sealed class PeripheralWithManagedThread : IDoubleWordPeripheral
        {
            public PeripheralWithManagedThread(Machine machine)
            {
                resetEvent = new ManualResetEventSlim(false);
                machine.ObtainManagedThread(ThreadAction, this, 0, "test", false).Start();
            }

            public uint ReadDoubleWord(long offset)
            {
                return 0;
            }

            public void WriteDoubleWord(long offset, uint value)
            {

            }

            public void Reset()
            {

            }

            public bool IsTheThreadRunning
            {
                get
                {
                    return resetEvent.Wait(500);
                }
            }

            private void ThreadAction()
            {
                resetEvent.Set();
                Thread.Sleep(100);
                resetEvent.Reset();
            }

            private readonly ManualResetEventSlim resetEvent;
        }
    }
}

