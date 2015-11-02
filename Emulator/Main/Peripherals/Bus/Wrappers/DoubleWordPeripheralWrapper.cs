//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.Bus.Wrappers
{
    internal class DoubleWordPeripheralWrapper : IDoubleWordPeripheral
    {
        public DoubleWordPeripheralWrapper(BusAccess.DoubleWordReadMethod read, BusAccess.DoubleWordWriteMethod write)
        {
            this.read = read;
            this.write = write;
        }

        public uint ReadDoubleWord(long offset)
        {
            return read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            write(offset, value);
        }

        public void Reset()
        {
        }

        private readonly BusAccess.DoubleWordReadMethod read;
        private readonly BusAccess.DoubleWordWriteMethod write;
    }
}

