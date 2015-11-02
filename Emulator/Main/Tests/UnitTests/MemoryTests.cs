//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using NUnit.Framework;
using Emul8.Peripherals;
using Emul8.Core;
using Emul8.Utilities;
using Emul8.Peripherals.Memory;

namespace Emul8.UnitTests
{
    [TestFixture]
    public class MemoryTests
    {
        [Test]
        public void ShouldReadWriteMemoryBiggerThan2GB()
        {
            const uint MemorySize = 3u * 1024 * 1024 * 1024;
            var memory = new MappedMemory(MemorySize);
            var machine = new Machine();
            var start = (long)100.MB();
            machine.SystemBus.Register(memory, start);
            var offset1 = start + 16;
            var offset2 = start + MemorySize - 16;
            machine.SystemBus.WriteByte(offset1, 0x1);
            machine.SystemBus.WriteByte(offset2, 0x2);

            Assert.AreEqual(0x1, machine.SystemBus.ReadByte(offset1));
            Assert.AreEqual(0x2, machine.SystemBus.ReadByte(offset2));
        }
    }
}

