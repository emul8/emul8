//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Utilities.GDB
{
    internal static class Helpers
    {
        public static uint SwapBytes(uint value)
        {
            var a = BitConverter.GetBytes(value);
            Array.Reverse(a);
            return BitConverter.ToUInt32(a, 0);
        }

        public static ushort SwapBytes(ushort value)
        {
            var a = BitConverter.GetBytes(value);
            Array.Reverse(a);
            return BitConverter.ToUInt16(a, 0);
        }
    }
}

