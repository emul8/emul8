﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.SPI
{
    //This is a dummy device, emulating connection between MISO and MOSI lines.
    public class SPILoopback : ISPIPeripheral
    {
        public void FinishTransmission()
        {
        }

        public void Reset()
        {
        }

        public byte Transmit(byte data)
        {
            return data;
        }
    }
}
