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
    public interface IDisplay
    {
        void DrawFrame(byte[] frame);
        void DrawFrame(IntPtr pointer);
        void SetDisplayParameters(int width, int height, PixelFormat colorFormat);
    }
}

