//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using Emul8.Backends.Display;

namespace Emul8.Peripherals.DMA
{
    internal enum Dma2DColorMode
    {
        ARGB8888,
        RGB888,
        RGB565,
        ARGB1555,
        ARGB4444,
        L8,
        AL44,
        AL88,
        L4,
        A8,
        A4
    }

    internal static class Dma2DColorModeExtensions
    {
        public static PixelFormat ToPixelFormat(this Dma2DColorMode mode)
        {
            return (PixelFormat)Enum.Parse(typeof(PixelFormat), mode.ToString());
        }
    }
}

