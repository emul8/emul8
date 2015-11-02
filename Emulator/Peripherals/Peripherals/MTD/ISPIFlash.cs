//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.MTD
{
    public interface ISPIFlash : IPeripheral
    {
        void WriteEnable();
        void WriteDisable();
        void WriteStatusRegister(uint registerNumber, uint value);
        uint ReadStatusRegister(uint registerNumber);
        uint ReadID();
    }

    public enum SPIFlashCommand
    {
        WriteEnable = 0x06,
        WriteDisable = 0x04,
        WriteToStatusRegister = 0x01,
        ReadStatusRegister = 0x05,
        Read = 0x03,
        FastRead = 0x0B,
        BulkErase = 0x60,
        ReadID = 0x9F,
        SectorErase = 0xD8,
        ChipErase = 0xC7,
        PageProgram = 0x02
    }
}

