//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Peripherals.Memory
{
    public class ArrayMemoryWithReadonlys : ArrayMemory
    {
        public ArrayMemoryWithReadonlys(int size):base(size)
        {
        }
        public ArrayMemoryWithReadonlys(byte[] source):base(source)
        {
        }

        public void SetReadOnlyDoubleWord(long offset, uint value)
        {
            WriteDoubleWord(offset, value);
            ignoreWrites.Add(offset);
        }

        public void SetReadOnlyWord(long offset, ushort value)
        {
            WriteWord(offset, value);
            ignoreWrites.Add(offset);
        }

        public void SetReadOnlyByte(long offset, byte value)
        {
            WriteByte(offset, value);
            ignoreWrites.Add(offset);
        }
        
        public override void WriteDoubleWord(long offset, uint value)
        {           
            if(!ignoreWrites.Contains(offset))
            {
                var bytes = BitConverter.GetBytes(value);
                bytes.CopyTo(array, offset);
            }
        }
        public override void WriteWord(long offset, ushort value)
        {
            if(!ignoreWrites.Contains(offset))
            {
                var bytes = BitConverter.GetBytes(value);
                bytes.CopyTo(array, offset);
            }
        }
        public override void WriteByte(long offset, byte value)
        { 
            if(!ignoreWrites.Contains(offset))
            {
                var intOffset = (int)offset;
                array[intOffset] = value;
            }
        }
        
        private HashSet<long> ignoreWrites = new HashSet<long>();
    }
}

