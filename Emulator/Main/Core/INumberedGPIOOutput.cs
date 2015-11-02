//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;

namespace Emul8.Core
{
    public interface INumberedGPIOOutput
    {
        IReadOnlyDictionary<int, IGPIO> Connections { get; }
    }
}

