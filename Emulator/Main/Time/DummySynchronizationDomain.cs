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
    public class DummySynchronizationDomain : ISynchronizationDomain
    {
        public ISynchronizer ProvideSynchronizer()
        {
            return new DummySynchronizer();
        }

        public void ExecuteOnNearestSync(Action action)
        {
            action();
        }

        public long SynchronizationsCount
        {
            get
            {
                return 0;
            }
        }

        public long SyncUnit { get; set; }

        public bool OnSyncPointThread
        {
            get
            {
                return false;
            }
        }

        private sealed class DummySynchronizer : ISynchronizer
        {
            public bool Sync()
            {
                return true;
            }

            public void CancelSync()
            {

            }

            public void RestoreSync()
            {

            }

            public void Exit()
            {

            }
        }
    }
}

