//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Exceptions;
using Emul8.Peripherals;

namespace Emul8.UnitTests.Mocks
{
    public class MockPeripheralWithDependency : IPeripheral
    {
        public MockPeripheralWithDependency(IPeripheral other = null, bool throwException = false)
        {
            if(throwException)
            {
                throw new ConstructionException("Fake exception");
            }
        }

        public void Reset()
        {
            
        }
    }
}
