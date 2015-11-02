//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Time
{
    public interface ISynchronizer
    {
        void Sync();
        void CancelSync();
        void RestoreSync();
        void Exit();
    }
}
