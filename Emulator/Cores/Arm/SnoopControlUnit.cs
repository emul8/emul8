//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using Emul8.Utilities;

namespace Emul8.Peripherals.Miscellaneous
{
    public class SnoopControlUnit : IDoubleWordPeripheral
    {
        public SnoopControlUnit(Machine machine)
        {
            this.machine = machine;
        }

        public uint ReadDoubleWord(long offset)
        {
            switch(offset)
            {
            case 0x0:
                return scu;
            case 0x4:
                //TODO: should work!
                var numOfCPUs = machine.SystemBus.GetCPUs().Count();
                return (uint)(0x30 + numOfCPUs - 1);//((0xffffffff << NumOfCPUs) ^ 0xffffffff) << 4 + NumOfCPUs - 1;// [7:4] - 1 for SMP, 0 for AMP, [1:0] - number of processors - 1
            }
            return 0;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            switch(offset)
            {
            case 0x0:
                scu = value & 1;
                return;
            }
            this.LogUnhandledWrite(offset, value);
        }

        public void Reset()
        {
            scu = 0;
        }

        private uint scu;
        private readonly Machine machine;
    }
}

