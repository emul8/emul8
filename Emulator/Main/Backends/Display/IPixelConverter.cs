//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Backends.Display
{
    public interface IPixelConverter
    {
        void Convert(byte[] inBuffer, byte[] clutBuffer, ref byte[] outBuffer);
        void Convert(byte[] inBuffer, ref byte[] outBuffer);

        PixelFormat Input { get; }
        PixelFormat Output { get; }
    }
}

