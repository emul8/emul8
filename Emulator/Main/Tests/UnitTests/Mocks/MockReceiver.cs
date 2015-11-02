//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Core;
using Emul8.Peripherals.Bus;

namespace UnitTests.Mocks
{
    public class MockReceiver : IGPIOReceiver, IBytePeripheral
	{
        public MockReceiver()
        {
            IRQ = new GPIO();
        }

		public void Reset()
		{
		}

		public void OnGPIO(int number, bool value)
		{
		}

        public GPIO IRQ
        {
            get;
            set;
        }

        public byte ReadByte(long offset)
        {
            throw new System.NotImplementedException();
        }

        public void WriteByte(long offset, byte value)
        {
            throw new System.NotImplementedException();
        }
	}
	
}
