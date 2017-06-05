//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using UnitTests.Mocks;

namespace Emul8.UnitTests.Mocks
{
    public class MockIrqSenderWithTwoInterrupts : MockIrqSender
    {
        public GPIO AnotherIrq { get; private set; }
    }
}
