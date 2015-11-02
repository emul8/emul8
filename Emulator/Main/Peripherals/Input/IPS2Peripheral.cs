//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Input
{
    public interface IPS2Peripheral : IPeripheral
    {
        byte Read();
        void Write(byte value);
        IPS2Controller Controller { get; set; }
    }
}

