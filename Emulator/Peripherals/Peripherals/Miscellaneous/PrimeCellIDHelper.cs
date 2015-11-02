//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Exceptions;
using Emul8.Logging;
using System.Linq;

namespace Emul8.Peripherals.Miscellaneous
{
    public sealed class PrimeCellIDHelper
    {
        public PrimeCellIDHelper(int peripheralSize, byte[] data, IPeripheral parent)
        {
            this.peripheralSize = peripheralSize;
            this.data = data.ToArray(); // to obtain a copy
            this.parent = parent;
            if(data.Length != 8)
            {
                throw new RecoverableException("You have to provide full peripheral id and prime cell id (8 bytes).");
            }
        }

        public byte Read(long offset)
        {
            if(offset >= peripheralSize || offset < peripheralSize - 8 * 4)
            {
                parent.LogUnhandledRead(offset);
                return 0;
            }
            return data[8 - (peripheralSize - offset)/4];
        }

        private readonly int peripheralSize;
        private readonly IPeripheral parent;
        private readonly byte[] data;
    }
}

