//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Network;

namespace Emul8.Peripherals.Wireless
{
    public interface IRadio : IPeripheral, INetworkInterface
    {
        int Channel { get; set; }
        event Action<IRadio, byte[]> FrameSent;
        void ReceiveFrame(byte[] frame);
    }
}

