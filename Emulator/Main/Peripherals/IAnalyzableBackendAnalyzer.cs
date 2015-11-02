//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;

namespace Emul8.Peripherals
{
    public interface IAnalyzableBackendAnalyzer<T> : IAnalyzableBackendAnalyzer where T : IAnalyzableBackend
    {
        void AttachTo(T backend);
    }

    public interface IAnalyzableBackendAnalyzer : IAutoLoadType
    {
        void Show();
        void Hide();
        string Id { get; }

        IAnalyzableBackend Backend { get; }
    }
}

