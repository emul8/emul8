//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Network;
using Emul8.Core;

namespace Emul8.HostInterfaces.Network
{
    public interface ITapInterface : IMACInterface, IHostMachineElement
    {
        string InterfaceName { get; }
    }
}

