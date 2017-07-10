//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using NUnit.Framework;
using Emul8.Peripherals.Timers;
using Emul8.Core;

namespace Emul8.UnitTests
{
    [TestFixture]
    public class ComparingTimerTests
    {
        [Test]
        public void ShouldThrowOnCompareHigherThanLimit()
        {
            var machine = new Machine();
            var timer = new ComparingTimer(machine, 10, 20, compare: 5);
            Assert.Throws<InvalidOperationException>(() => timer.Compare = 30);
        }

        [Test]
        public void ShouldThrowOnNegativeCompare()
        {
            var machine = new Machine();
            var timer = new ComparingTimer(machine, 10, 20, compare: 5);
            Assert.Throws<InvalidOperationException>(() => timer.Compare = -2);
        }
    }
}

