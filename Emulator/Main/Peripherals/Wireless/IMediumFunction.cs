//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Wireless
{
    public interface IMediumFunction
    {
        bool CanReach(Position from, Position to);
    }
}
