//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;

namespace Emul8.Peripherals.MTD
{
    public class DummySPIFlash: ISPIFlash
    {
        #region IPeripheral implementation

        public void Reset()
        {
        }

        #endregion

        #region ISPIFlash implementation

        public void WriteEnable()
        {
            throw new NotImplementedException();
        }

        public void WriteDisable()
        {
            throw new NotImplementedException();
        }

        public void WriteStatusRegister(uint registerNumber, uint value)
        {
            throw new NotImplementedException();
        }

        public uint ReadStatusRegister(uint registerNumber)
        {
            throw new NotImplementedException();
        }

        public uint ReadID()
        {
            this.Log(LogLevel.Warning,"Reading ID");
            return 0xffffffff;
        }

        #endregion

        public DummySPIFlash()
        {
        }
    }
}

