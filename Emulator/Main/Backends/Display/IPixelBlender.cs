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
    public interface IPixelBlender
    {
        void Blend(byte[] backBuffer, byte[] frontBuffer, ref byte[] output, Pixel background = null, byte backBufferAlphaMultiplayer = 0xFF, byte frontBufferAlphaMultiplayer = 0xFF);

        PixelFormat BackBuffer { get; }
        PixelFormat FrontBuffer { get; }
        PixelFormat Output { get; }
    }
}

