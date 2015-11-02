//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Network;
using Emul8.UserInterface;

namespace Emul8.Peripherals.Network
{
    [Icon("network")]
    public interface INetworkInterface : IEmulationElement, IAnalyzable
	{
        NetworkLink Link{get;}
        void ReceiveFrame(EthernetFrame frame);
	}
}

