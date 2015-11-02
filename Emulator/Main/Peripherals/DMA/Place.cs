//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.DMA
{
    public class Place
    {
        public Place(byte[] array, int startIndex)
        {
            Array = array;
            StartIndex = startIndex;
        }

        public Place(long address)
        {
            Address = address;
        }

        public long? Address { get; private set; }
        public byte[] Array { get; private set; }
        public int? StartIndex { get; private set; }

        public static implicit operator Place(long address)
        {
            return new Place(address);
        }
    }
}

