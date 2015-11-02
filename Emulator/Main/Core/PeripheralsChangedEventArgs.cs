//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;

namespace Emul8.Core
{
    public class PeripheralsChangedEventArgs 
    {
        public PeripheralsChangedEventArgs(IPeripheral peripheral, PeripheralChangeType operation)
        {
            Peripheral = peripheral;
            Operation = operation;
        }

        public IPeripheral Peripheral { get; private set; }
        public PeripheralChangeType Operation { get; private set; }

        public enum PeripheralChangeType
        {
            Addition,
            Removal,
            CompleteRemoval,
            NameChanged
        }
    }
}

