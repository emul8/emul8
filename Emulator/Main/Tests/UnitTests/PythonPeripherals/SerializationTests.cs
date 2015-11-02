//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using NUnit.Framework;
using Antmicro.Migrant;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.Python;
using System.IO;
using IronPython.Runtime;
using Emul8.Utilities;

namespace UnitTests.PythonPeripherals
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void ShouldSerializeSimplePyDev()
        {
            var source = @"
if request.isInit:
	num = 4
	str = 'napis'
if request.isRead:
	request.value = num + len(str)
";
            var pyDev = PythonPeripheral.FromString(source, 100, true);
            var copy = Serializer.DeepClone(pyDev);
            Assert.AreEqual(9, copy.ReadByte(0));
        }

        [Test]
        public void ShouldSerializePyDevWithListAndDictionary()
        {
            var source = @"
if request.isInit:
	some_list = [ 'a', 666 ]
	some_dict = { 'lion': 'yellow', 'kitty': 'red' }
if request.isRead:
	if request.offset == 0:
		request.value = some_list[1]
	if request.offset == 4:
		request.value = len(some_dict['kitty'])
";
            var pyDev = PythonPeripheral.FromString(source, 100, true);
            var serializer = new Serializer();
            serializer.ForObject<PythonDictionary>().SetSurrogate(x => new PythonDictionarySurrogate(x));
            serializer.ForSurrogate<PythonDictionarySurrogate>().SetObject(x => x.Restore());
            var mStream = new MemoryStream();
            serializer.Serialize(pyDev, mStream);
            mStream.Seek(0, SeekOrigin.Begin);
            var copy = serializer.Deserialize<PythonPeripheral>(mStream);
            Assert.AreEqual(666, copy.ReadDoubleWord(0));
            Assert.AreEqual(3, copy.ReadDoubleWord(4));
        }

        [Test]
        public void ShouldSerializePyDevWithModuleImport()
        {
            var source = @"
import time

if request.isRead:
	request.value = time.localtime().tm_hour
";
            var pyDev = PythonPeripheral.FromString(source, 100, true);
            var copy = Serializer.DeepClone(pyDev);
            Assert.AreEqual(CustomDateTime.Now.Hour, copy.ReadDoubleWord(0), 1);
        }

        [Test]
        public void ShouldSerializeMachineWithSimplePyDev()
        {            
            var source = @"
if request.isInit:
    num = 4
    str = 'napis'
if request.isRead:
    request.value = num + len(str)
";
            var pyDev = PythonPeripheral.FromString(source, 100, true);
            var sysbus = machine.SystemBus;
            sysbus.Register(pyDev, new Emul8.Peripherals.Bus.BusRangeRegistration(0x100, 0x10));

            Assert.AreEqual(9, sysbus.ReadByte(0x100));
            machine = Serializer.DeepClone(machine);
            sysbus = machine.SystemBus;
            Assert.AreEqual(9, sysbus.ReadByte(0x100));
        }

        [SetUp]
        public void SetUp()
        {
            EmulationManager.Instance.Clear();
            machine = new Machine();
        }

        private Machine machine;
    }
}
