//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Backends.Display
{
    public class Pixel
    {
        public Pixel(byte red, byte green, byte blue, byte alpha)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public byte Alpha { get; private set; }
        public byte Red   { get; private set; }
        public byte Green { get; private set; }
        public byte Blue  { get; private set; }
    }
}

