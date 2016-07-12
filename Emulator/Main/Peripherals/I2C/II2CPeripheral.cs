//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.I2C
{
    public interface II2CPeripheral : IPeripheral
    {
        void Write(byte[] data);
        byte[] Read(int count = 1);
    }
}

