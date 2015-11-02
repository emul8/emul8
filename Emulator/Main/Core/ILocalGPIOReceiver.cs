//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core
{
    public interface ILocalGPIOReceiver
    {
        IGPIOReceiver GetLocalReceiver(int index);
    }
}

