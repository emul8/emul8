//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Core.Structure;
using Emul8.Peripherals;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.CPU;

namespace Emul8.UnitTests.Mocks
{
    public class NullRegister : IPeripheralRegister<ICPU, NullRegistrationPoint>, IDoubleWordPeripheral
    {
        public NullRegister(Machine machine)
        {
            this.machine = machine;
        }

        public void Register(ICPU peripheral, NullRegistrationPoint registrationPoint)
        {
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Unregister(ICPU peripheral)
        {
            machine.UnregisterAsAChildOf(this, peripheral);
        }

        public void Reset()
        {
            
        }

        public uint ReadDoubleWord(long offset)
        {
            return 0;
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            
        }

        private readonly Machine machine;
    }
}
