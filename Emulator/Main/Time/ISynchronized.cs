//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;

namespace Emul8.Time
{
    public interface ISynchronized : IInterestingType
    {
        ISynchronizationDomain SyncDomain { get; set; }
    }
}

