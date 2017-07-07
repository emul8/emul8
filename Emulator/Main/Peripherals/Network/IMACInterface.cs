//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Core.Structure;
using Emul8.Network;

namespace Emul8.Peripherals.Network
{
    public interface IMACInterface : INetworkInterface
    {
        MACAddress MAC { get; set; }
        NetworkLink Link { get; }
        void ReceiveFrame(EthernetFrame frame);
    }
}

