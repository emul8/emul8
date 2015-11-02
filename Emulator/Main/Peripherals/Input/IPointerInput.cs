//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;

namespace Emul8.Peripherals.Input
{
    public interface IPointerInput : IInputDevice
    {
        void Press(MouseButton button = MouseButton.Left);
        void Release(MouseButton button = MouseButton.Left);
    }
}


