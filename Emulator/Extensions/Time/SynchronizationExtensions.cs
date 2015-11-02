//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Hooks;

namespace Emul8.Time
{
    public static class SynchronizationExtensions
    {
        // note that this function only exists to provide access to feature
        // that is normally not accessible due to the limitations of the monitor
        public static void SetSyncDomainFromEmulation(this ISynchronized @this, int domainIndex)
        {
            @this.SyncDomain = EmulationManager.Instance.CurrentEmulation.SyncDomains[domainIndex];
        }

        // again, exists only for monitor
        public static void SetHookAtSyncPoint(this Emulation emulation, int domainIndex, string handler)
        {
            var engine = new SyncPointHookPythonEngine(handler, emulation);
            ((SynchronizationDomain)emulation.SyncDomains[domainIndex]).SetHookOnSyncPoint(engine.Hook);
        }

        // and again
        public static void ClearHookAtSyncPoint(this Emulation emulation, int domainIndex)
        {
            ((SynchronizationDomain)emulation.SyncDomains[domainIndex]).ClearHookOnSyncPoint();
        }
    }
}

