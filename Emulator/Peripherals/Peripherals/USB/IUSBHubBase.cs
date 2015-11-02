//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.USB
{
    public interface IUSBHubBase : IPeripheralRegister<IUSBHub, USBRegistrationPoint>,  IPeripheralContainer<IUSBPeripheral, USBRegistrationPoint>
    {
         event Action <uint> Connected ;
         event Action <uint,uint> Disconnected ;
         event Action <IUSBHub> RegisterHub ;
         event Action <IUSBPeripheral> ActiveDevice ;
    }
}
