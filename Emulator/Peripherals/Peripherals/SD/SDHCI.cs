//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using Emul8.Storage;
using Emul8.Utilities;
using System.IO;
using Emul8.Core;

namespace Emul8.Peripherals.SD
{
    public sealed class SDHCI : IBytePeripheral, IWordPeripheral, IDoubleWordPeripheral, IDisposable
    {        
        public SDHCI(/*string fileName, int size, BusWidth bits = BusWidth.Bits32, bool nonPersistent = false*/)
        {
        }
        
        public byte ReadByte(long offset)
        {
                this.LogUnhandledRead(offset);
                return 0;
        }

        public ushort ReadWord(long offset)
        {
            this.LogUnhandledRead(offset);
            return 0;
        }

        public uint ReadDoubleWord(long offset)
        {
            switch (offset) {
                case 0x24: // SDHC_PRNSTS
                        this.Log(LogLevel.Warning, "Read from 0x24 - SDHC_PRNSTS");
                        return 0xFFFFFFFF;
                default:
                        this.LogUnhandledRead(offset);
                        break;
            }
            return 0;
        }

        public void WriteByte(long offset, byte value)
        {
            this.LogUnhandledWrite(offset, value);
        }

        public void WriteWord(long offset, ushort value)
        {
            this.LogUnhandledWrite(offset, value);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            this.LogUnhandledWrite(offset, value);
        }

        public void Reset()
        {
        }
        
        public void Dispose()
        {
        }
        
    }
}

