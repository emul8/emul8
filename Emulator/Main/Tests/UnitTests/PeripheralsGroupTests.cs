//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using Emul8.Peripherals;
using NUnit.Framework;
using Emul8.Exceptions;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.Memory;

namespace Emul8.UnitTests
{
    [TestFixture]
    public class PeripheralsGroupTests
    {
        [Test]
        public void ShouldNotUnregisterSinglePeripheralFromGroup()
        {
            using(var machine = new Machine())
            {
                var peripheral = new MappedMemory(10);
                machine.SystemBus.Register(peripheral, new BusPointRegistration(0x0));
                machine.SystemBus.Unregister(peripheral);

                machine.SystemBus.Register(peripheral, new BusPointRegistration(0x0));
                machine.PeripheralsGroups.GetOrCreate("test-group", new [] { peripheral });

                try 
                {
                    machine.SystemBus.Unregister(peripheral);
                }
                catch (RegistrationException)
                {
                    return;
                }

                Assert.Fail();
            }
        }

        [Test]
        public void ShouldUnregisterPeripheralGroups()
        {
            using(var machine = new Machine())
            {
                var peripheral = new MappedMemory(10);
                machine.SystemBus.Register(peripheral, new BusPointRegistration(0x0));
                var group = machine.PeripheralsGroups.GetOrCreate("test-group", new [] { peripheral });
                Assert.IsTrue(machine.IsRegistered(peripheral));
                group.Unregister();
                Assert.IsFalse(machine.IsRegistered(peripheral));
            }
        }

        [Test]
        public void ShouldNotAddUnregisteredPeripheralToGroup()
        {
            using(var machine = new Machine())
            {
                try 
                {
                    machine.PeripheralsGroups.GetOrCreate("test-group", new [] { new MappedMemory(10) });
                }
                catch (RegistrationException)
                {
                    return;
                }

                Assert.Fail();
            }
        }
    }
}

