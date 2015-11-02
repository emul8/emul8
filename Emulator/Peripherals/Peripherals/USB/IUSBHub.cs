//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.USB
{
    public interface IUSBHub : IUSBPeripheral, IUSBHubBase
    {
        IUSBPeripheral GetDevice(byte port);
        IUSBHub Parent{ set; }
        byte NumberOfPorts { get; set; }
    }
}
