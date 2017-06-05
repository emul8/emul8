//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Exceptions;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.I2C;

namespace UnitTests.Mocks
{
	public class EmptyPeripheral : II2CPeripheral, IBytePeripheral, IDoubleWordPeripheral
	{
        // TODO: more interfaces

        public EmptyPeripheral()
        {
            Increment();
        }

        public EmptyPeripheral(double value)
        {
            Increment();
            Increment();
        }

        public EmptyPeripheral(int value, bool enabled = false)
        {
            Increment();
            Increment();
            Increment();
        }
		
		public virtual void Reset()
		{
			
		}
		
		public byte[] Read (int count)
		{
			return new byte[]{0};
		}

		public void Write (byte[] data)
		{
			
		}

		public byte ReadByte (long offset)
		{
			return 0;
		}

		public void WriteByte (long offset, byte value)
		{

		}

		public virtual uint ReadDoubleWord(long offset)
		{
			return 0;
		}
		
		public virtual void WriteDoubleWord(long offset, uint value)
		{
			
		}

        public void Increment()
        {
            Counter++;
        }

        public int ThrowingProperty
        {
            get
            {
                return 0;
            }
            set
            {
                throw new RecoverableException("Fake exception");
            }
        }

        public bool BooleanProperty { get; set; }

        public int Counter { get; private set; }
	}
}

