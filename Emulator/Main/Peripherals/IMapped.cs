//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using Emul8.Peripherals.Bus;
using Emul8.Core;

namespace Emul8.Peripherals
{
    public interface IMapped : IBusPeripheral
    {
        IEnumerable<IMappedSegment> MappedSegments { get; }
    }
}

