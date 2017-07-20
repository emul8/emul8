//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core.Structure;
using Emul8.Peripherals.CPU;
using Emul8.Utilities;

namespace Emul8.UnitTests.Mocks
{
    public class EmptyInterestingType : IPeripheralRegister<ICPU, NullRegistrationPoint>
    {
        // note: Register and Unregister methods are empty, because the purpose of this type is to test
        // casting of types (that is why this type does not implement IPeripheral) and they will not be used

        public void Register(ICPU peripheral, NullRegistrationPoint registrationPoint)
        {
        }

        public void Unregister(ICPU peripheral)
        {
        }
    }
}
