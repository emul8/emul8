//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using System.Collections.Generic;

namespace Emul8.Backends.Display
{
    public enum PixelFormat
    {
        A4,
        L4,
        A8,
        L8,
        AL44,
        AL88,
        RGB565,
        BGR565,
        BGR888,
        RGB888,
        ARGB1555,
        ARGB4444,
        RGBA4444,
        ABGR4444,
        BGRA4444,
        XRGB4444,
        RGBX4444,
        XBGR4444,
        BGRX4444,
        BGRA8888,
        RGBA8888,
        ABGR8888,
        ARGB8888,
        BGRX8888,
        RGBX8888,
        XRGB8888,
        XBGR8888
    }

    public enum ColorType
    {
        R,
        G,
        B,
        A,
        X,
        L
    }

    public static class PixelFormatExtensions
    {
        /// <summary>
        /// Calculates the number of bytes needed to encode the color.
        /// </summary>
        /// <param name="format">Color format.</param>
        public static int GetColorDepth(this PixelFormat format)
        {
            switch(format)
            {
	    case PixelFormat.A8:
	        return 1;
            case PixelFormat.RGB565:
            case PixelFormat.BGR565:
            case PixelFormat.ARGB4444:
            case PixelFormat.ABGR4444:
            case PixelFormat.BGRA4444:
            case PixelFormat.RGBA4444:
            case PixelFormat.XRGB4444:
            case PixelFormat.XBGR4444:
            case PixelFormat.BGRX4444:
            case PixelFormat.RGBX4444:
                return 2;
            case PixelFormat.BGR888:
            case PixelFormat.RGB888:
                return 3;
            case PixelFormat.BGRA8888:
            case PixelFormat.RGBA8888:
            case PixelFormat.ABGR8888:
            case PixelFormat.ARGB8888:
            case PixelFormat.BGRX8888:
            case PixelFormat.RGBX8888:
            case PixelFormat.XRGB8888:
            case PixelFormat.XBGR8888:
                return 4;
            default:
                throw new ArgumentOutOfRangeException("format");
            }
        }

        /// <summary>
        /// Calculates lenghts (in bits) of colors deduced from format name.
        /// </summary>
        /// <returns>Colors maped to a number of bits they are encoded at.</returns>
        /// <param name="format">Color format</param>
        public static Dictionary<ColorType, byte> GetColorsLengths(this PixelFormat format)
        {
            var bits = new Dictionary<ColorType, byte>();

            var offset = 0;
            var formatAsCharArray = format.ToString().ToCharArray();
            var firstNumberPosition = formatAsCharArray.Length / 2;

            while (offset < firstNumberPosition)
            {
                ColorType colorType;
                if(!Enum.TryParse(formatAsCharArray[offset].ToString(), out colorType))
                {
                    throw new ArgumentException();
                }

                bits.Add(colorType, (byte)(formatAsCharArray[firstNumberPosition + offset] - '0'));
                offset++;
            }

            return bits;
        }
    }
}

