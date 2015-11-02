//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.USB
{
    public enum DescriptorType : byte
    {
        Device = 1,
        Configuration = 2,
        String = 3,
        Intreface = 4,
        Endpoint = 5,
        DeviceQualifier = 6,
        OtherSpeedConfiguration = 7,
        InterfacePower = 8
    }


}

