//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Core;

namespace UnitTests.Mocks
{
	[GPIO(NumberOfInputs = 5)]
	public class MockReceiverConstrained : IGPIOReceiver
	{
		public void Reset()
		{
		}

		public void OnGPIO(int number, bool value)
		{

		}
	}
}
