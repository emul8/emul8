//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.SPI;

namespace Emul8.Peripherals.Sensors
{
    public class TI_LM74 : ISPIPeripheral
    {
        public TI_LM74()
        {
            Reset();
        }

        public void FinishTransmission()
        {
            Reset();
        }

        public void Reset()
        {
            isFirstByte = true;
            currentReadOut = 0;
        }

        public byte Transmit(byte data)
        {
            byte value = 0;
            if(isFirstByte)
            {
                //The 3 LSB are set to 1. 0x1000 = 0.0625C. Decimal->Int->UInt conversion to handle negative values.
                currentReadOut = (((uint)(int)(Temperature * 10000 / 625) << 3) | 0x7);
                value = (byte)(currentReadOut >> 8);
            }
            else
            {
                value = (byte)(currentReadOut & 0xFF);
            }
            isFirstByte = !isFirstByte;
            return value;
        }

        public decimal Temperature { get; set; }

        private uint currentReadOut;
        private bool isFirstByte;
    }
}
