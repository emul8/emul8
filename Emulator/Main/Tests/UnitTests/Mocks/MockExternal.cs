//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Time;

namespace Emul8.UnitTests.Mocks
{
    public sealed class MockExternal : SynchronizedExternalBase
    {
        public void OnNearestSync(Action action)
        {
            ExecuteOnNearestSync(action);
        }
    }
}

