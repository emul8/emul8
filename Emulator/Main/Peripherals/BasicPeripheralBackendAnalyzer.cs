//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals
{
    public abstract class BasicPeripheralBackendAnalyzer<T> : IAnalyzableBackendAnalyzer<T> where T : IAnalyzableBackend
    {
        public virtual void AttachTo(T backend)
        {
            Backend = backend;
        }

        public abstract void Show();
        public abstract void Hide();

        public abstract string Id { get; }

        public IAnalyzableBackend Backend { get; private set; }
    }
}

