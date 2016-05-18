//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Wireless.CC2538
{
    internal enum AddressingMode : byte
    {
        None = 0x0,
        Reserved = 0x1,
        // 2 bytes PAN id, 2 bytes address
        ShortAddress = 0x2,
        // 2 bytes PAN, 8 bytes address
        ExtendedAddress = 0x3
    }

    internal static class AddressingModeExtensions
    {
        public static int GetBytesLength(this AddressingMode mode)
        {
            switch(mode)
            {
                case AddressingMode.None:
                    return 0;
                case AddressingMode.ShortAddress:
                    return 2;
                case AddressingMode.ExtendedAddress:
                    return 8;
                default:
                    throw new ArgumentException();
            }
        }
    }
}

