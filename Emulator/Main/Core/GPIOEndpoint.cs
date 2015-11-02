//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core
{
    public sealed class GPIOEndpoint
    {
        public GPIOEndpoint(IGPIOReceiver receiver, int number)
        {
            Receiver = receiver;
            Number = number;
        }

        public IGPIOReceiver Receiver { get; private set; }
        public int Number { get; private set; }
    }
}

