//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.UserInterface;

namespace Emul8.Peripherals.CAN
{
    [Icon("can")]
    public interface ICAN : IPeripheral
    {
        event Action<int, byte[]> FrameSent;
        void OnFrameReceived(int id, byte[] data);
    }
}

