//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Logging;
using Emul8.Peripherals.Bus;

namespace Emul8.Peripherals.Cache
{
    public class PL310 : IDoubleWordPeripheral
    {
        public uint ReadDoubleWord(long offset)
        {
            switch((Offset)offset)
            {
            case Offset.CacheId:
                return 0x410000C0;
            default:
                this.LogUnhandledRead(offset);
                break;
            }
            return 0;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            switch((Offset)offset)
            {
            case Offset.CacheSync:
            case Offset.CacheSyncProbably:
            case Offset.InvalidateLineByPA:
            case Offset.CleanLinebyPA:
            case Offset.CleanAndInvalidateLine:
            case Offset.Debug:
                break;
            default:
                this.LogUnhandledWrite(offset, value);
                break;
            }
        }

        public void Reset()
        {

        }

        public enum Offset
        {
            CacheId = 0x0,
            CacheSync = 0x730,
            CacheSyncProbably = 0x740, // TODO: check this offset,
            InvalidateLineByPA = 0x770,
            CleanLinebyPA = 0x7b0,
            CleanAndInvalidateLine = 0x7f0,
            Debug = 0xf40
        }

    }
}

