//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Bus;
using Emul8.Core;
using Emul8.Logging;

namespace Emul8.Peripherals.MTD
{
    [AllowedTranslations(AllowedTranslation.ByteToDoubleWord)]
    public class FSLNAND : IDoubleWordPeripheral, IKnownSize
    {
        public FSLNAND()
        {
            IRQ = new GPIO();
        }

        public long Size
        {
            get
            {
                return 0x10000;
            }
        }

        public GPIO IRQ { get; private set; }

        public uint ReadDoubleWord(long offset)
        {
            if(offset == 0x3F38)
            {
                return 0xFFFFFFFF;
            }
            this.LogUnhandledRead(offset);
            return 0;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            if(offset == 0x3F04)
            {
                IRQ.Set();
            }
            else if(offset == 0x3F38)
            {
                IRQ.Unset();
            }
            else
            {
                this.LogUnhandledWrite(offset, value);
            }
        }

        public void Reset()
        {
        }

    }
}

