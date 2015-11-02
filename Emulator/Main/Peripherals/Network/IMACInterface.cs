//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Core.Structure;

namespace Emul8.Peripherals.Network
{
    public interface IMACInterface : INetworkInterface
    {
        MACAddress MAC { get; set; }
    }
}

