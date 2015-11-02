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
using Emul8.Utilities;

namespace Emul8.Peripherals
{
    [AllowedTranslations(AllowedTranslation.DoubleWordToWord)]
    public class EfmSystemDevice: IBytePeripheral, IWordPeripheral
    {
        public EfmSystemDevice(byte family, ushort partNo, ushort flash, ushort ram, byte rev)
        {
            familyName = family;
            flashSize = flash;
            ramSize = ram;
            productRevision = rev;
            partNumber = partNo;
        }
     
        public void Reset()
        {
            // nothing happens
        }

        public byte ReadByte(long offset)
        {
            switch((EFMOffset)offset)
            {
            case EFMOffset.FamilyNameAdr:
                return familyName; 
            case EFMOffset.ProductRevisionAdr:
                return productRevision;
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }
        }

        public void WriteByte(long offset, byte value)
        {
            throw new NotImplementedException();
        }

        public ushort ReadWord(long offset)
        {
            switch((EFMOffset)offset)
            {
            case EFMOffset.PartNumberAdr:
                return partNumber;
            case EFMOffset.FlashSizeAdr:
                return flashSize;
            case EFMOffset.RamSizeAdr:
                return ramSize;
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }
        }
     
        public void WriteWord(long offset, ushort value)
        {
            throw new NotImplementedException();
        }

        private byte familyName;
        private ushort flashSize;
        private ushort ramSize;
        private byte productRevision;
        private ushort partNumber;

        private enum EFMOffset:long
        {
            FamilyNameAdr      = 0xe,
            FlashSizeAdr       = 0x8,
            RamSizeAdr         = 0xa,
            ProductRevisionAdr = 0xf,
            PartNumberAdr      = 0xc
        }
    }
}
