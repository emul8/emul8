//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.SPI
{
    public interface ISPIPeripheral : IPeripheral
    {
        byte Transmit(byte data);
        void FinishTransmission();
    }
}

