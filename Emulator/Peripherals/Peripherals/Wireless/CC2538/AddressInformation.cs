//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;

namespace Emul8.Peripherals.Wireless.CC2538
{
    internal class AddressInformation
    {
        public AddressInformation(AddressingMode destinationAddressingMode, AddressingMode sourceAddressingMode, bool intraPan, ArraySegment<byte> data)
        {
            var internalOffset = 0;
            if(destinationAddressingMode != AddressingMode.None)
            {
                DestinationPan = GetValue(data, 0);
                DestinationAddress = new Address(new ArraySegment<byte>(data.Array, data.Offset + 2, destinationAddressingMode.GetBytesLength()));
                internalOffset = 2 + DestinationAddress.Bytes.Count;
            }

            if(sourceAddressingMode != AddressingMode.None)
            {
                if(intraPan)
                {
                    SourcePan = DestinationPan;
                }
                else
                {
                    SourcePan = GetValue(data, internalOffset);
                    internalOffset += 2;
                }
                SourceAddress = new Address(new ArraySegment<byte>(data.Array, data.Offset + internalOffset, sourceAddressingMode.GetBytesLength()));
            }

            Bytes = data;
        }

        public uint SourcePan { get; private set; }
        public Address SourceAddress { get; private set; }
        public uint DestinationPan { get; private set; }
        public Address DestinationAddress { get; private set; }

        public ICollection<byte> Bytes { get; private set; }

        private static uint GetValue(IList<byte> data, int offset)
        {
            return (uint)(data[offset] + (data[offset + 1] << 8));
        }
    }
}

