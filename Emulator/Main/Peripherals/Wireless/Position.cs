//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Wireless
{
    public struct Position
    {
        public Position(decimal x, decimal y, decimal z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public decimal X { get; private set; }
        public decimal Y { get; private set; }
        public decimal Z { get; private set; }
    }
}
