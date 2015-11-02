//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.Bus.Wrappers
{
    internal class WordPeripheralWrapper : IWordPeripheral
    {
        public WordPeripheralWrapper(BusAccess.WordReadMethod read, BusAccess.WordWriteMethod write)
        {
            this.read = read;
            this.write = write;
        }

        public ushort ReadWord(long offset)
        {
            return read(offset);
        }

        public void WriteWord(long offset, ushort value)
        {
            write(offset, value);
        }

        public void Reset()
        {
        }

        private readonly BusAccess.WordReadMethod read;
        private readonly BusAccess.WordWriteMethod write;
    }
}

