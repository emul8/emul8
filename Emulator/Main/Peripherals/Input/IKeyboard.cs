//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Input
{
    public interface IKeyboard : IInputDevice
    {
        void Press(KeyScanCode scanCode);
        void Release(KeyScanCode scanCode);
    }
}

