//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Antmicro.Migrant;

namespace Emul8.Peripherals.UART
{
    public class SemihostingUart : IPeripheral, IUART
    {
        public SemihostingUart()
        {
            Reset();
        }

        [field: Transient]
        public event Action<byte> CharReceived;

        public Bits StopBits 
        { 
                get 
                { 
                        return 0;
                }
        }

        public Parity ParityBit
        {
            get
            {
                return Parity.None;
            }
        }

        public uint BaudRate
        {
            get
            {
                return 9600;
            }
        }

        public void Reset()
        {
            readFifo = new Queue<uint>(receiveFifoSize);    // typed chars are stored here
        }

        public void WriteChar(byte value) // char is typed
        {
            lock(UartLock)
            {
                readFifo.Enqueue(value);
            }
        }

	public byte SemihostingGetByte() {
		lock(UartLock) {
			return readFifo.Count > 0 ? (byte)readFifo.Dequeue() : (byte)0;
		}
	}

        public void SemihostingWriteString(string s)
        {
            lock(UartLock) {
	            for (int i = 0; i < s.Length; i++) {
                        OnCharReceived(Convert.ToByte(s[i]));
		    }
	    }
        }

        private void OnCharReceived(byte b)
        {
            var handler = CharReceived;
            if(handler != null)
            {
                handler(b);
            }

        }

        private object UartLock = new object();
        private Queue<uint> readFifo;
        private const int receiveFifoSize = 16;
    }
}

