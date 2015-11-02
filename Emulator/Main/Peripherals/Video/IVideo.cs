//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Backends.Display;
using ELFSharp.ELF;

namespace Emul8.Peripherals.Video
{
    public interface IVideo : IPeripheral
    {
        event Action<byte[]> FrameRendered;
        event Action<int, int, PixelFormat, Endianess> ConfigurationChanged;
    }
}

