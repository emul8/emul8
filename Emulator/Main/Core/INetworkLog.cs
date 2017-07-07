//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core
{
    public interface INetworkLog<out T> : IExternal
    {
        event Action<IExternal, T, T, byte[]> FrameTransmitted;
        event Action<IExternal, T, byte[]> FrameProcessed;
    }
}

