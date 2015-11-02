//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Peripherals;

namespace Emul8.Peripherals.Bus
{
	public class BusRegistered<T> : IBusRegistered<T> where T : IBusPeripheral
	{
		public BusRegistered(T what, BusRangeRegistration where)
        {			
			Peripheral = what;
            RegistrationPoint = where;
		}

		public T Peripheral { get; private set; }
        public BusRangeRegistration RegistrationPoint { get; private set; }
		
	}
}

