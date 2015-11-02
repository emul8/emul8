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

    public interface IRelativePositionPointerInput : IPointerInput
    {
        void MoveBy(int x, int y);
    }
}
