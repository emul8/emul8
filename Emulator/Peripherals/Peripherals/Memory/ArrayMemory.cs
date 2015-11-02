//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals;
using Emul8.Utilities;
using Emul8.Logging;
using System.Collections.Generic;

namespace Emul8.Peripherals.Memory
{
    public class ArrayMemory : IBytePeripheral, IWordPeripheral, IDoubleWordPeripheral, IKnownSize, IMemory
    {		
        public ArrayMemory(byte[] source)
        {
            array = source;
        }

        public ArrayMemory(int size)
        {
            array = new byte[size];
        }

      
        public uint ReadDoubleWord(long offset)
        {
            var intOffset = (int)offset;
            var result = BitConverter.ToUInt32(array, intOffset);
            return result;
        }

        public virtual void WriteDoubleWord(long offset, uint value)
        {		
            var bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(array, offset);

        }

        public void Reset()
        {
            // nothing happens
        }

        public ushort ReadWord(long offset)
        {
            var intOffset = (int)offset;
            var result = BitConverter.ToUInt16(array, intOffset);
            return result;
        }

        public virtual void WriteWord(long offset, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(array, offset);
        }

        public byte ReadByte(long offset)
        {
            var intOffset = (int)offset;
            var result = array[intOffset];
            return result;
        }

        public virtual void WriteByte(long offset, byte value)
        { 
            var intOffset = (int)offset;
            array[intOffset] = value;
        }

        public long Size
        {
            get
            {
                return array.Length;
            }
        }

        protected byte[] array;
    }
}

