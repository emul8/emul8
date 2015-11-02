//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.Bus.Wrappers
{
    internal class BytePeripheralWrapper : IBytePeripheral
    {
        public BytePeripheralWrapper(BusAccess.ByteReadMethod read, BusAccess.ByteWriteMethod write)
        {
            this.read = read;
            this.write = write;
        }

        public byte ReadByte(long offset)
        {
            return read(offset);
        }

        public void WriteByte(long offset, byte value)
        {
            write(offset, value);
        }

        public void Reset()
        {
        }

        private readonly BusAccess.ByteReadMethod read;
        private readonly BusAccess.ByteWriteMethod write;
    }
}

