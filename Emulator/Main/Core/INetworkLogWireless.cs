//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Utilities;
using Emul8.Peripherals.Wireless;

namespace Emul8.Core
{
    public interface INetworkLogWireless : INetworkLog<IRadio>, IInterestingType
    {
    }
}

