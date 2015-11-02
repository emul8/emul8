//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;

namespace Emul8.UserInterface
{
    public interface IUserInterfaceProvider
    {
        void ShowAnalyser(IAnalyzableBackendAnalyzer analyzer, string name);
        void HideAnalyser(IAnalyzableBackendAnalyzer analyzer);
    }
}

