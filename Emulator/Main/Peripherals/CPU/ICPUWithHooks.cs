//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.CPU
{
    public interface ICPUWithHooks : ICPU
    {
        void ClearHookAtBlockBegin();
        void SetHookAtBlockBegin(Action<uint, uint> hook);

        void AddHook(uint addr, Action<uint> hook);
        void RemoveHook(uint addr, Action<uint> hook);
        void RemoveHooksAt(uint addr);
        void RemoveAllHooks();
    }
}

