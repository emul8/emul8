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
    public abstract class SynchronizedExternalBase : ISynchronized
    {
        protected SynchronizedExternalBase()
        {
            SyncDomain = new DummySynchronizationDomain();
        }

        public ISynchronizationDomain SyncDomain { get; set; }

        protected void ExecuteOnNearestSync(Action action)
        {
            SyncDomain.ExecuteOnNearestSync(action);
        }
    }
}

