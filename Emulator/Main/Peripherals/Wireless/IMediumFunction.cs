//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;

namespace Emul8.Peripherals.Wireless
{
    public interface IMediumFunction : IEmulationElement
    {
        string FunctionName { get; }
        bool CanReach(Position from, Position to);
        bool CanTransmit(Position from);
    }
}
