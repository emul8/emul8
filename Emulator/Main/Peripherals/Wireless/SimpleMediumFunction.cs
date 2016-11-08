//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Wireless
{
    public sealed class SimpleMediumFunction : IMediumFunction
    {
        public static SimpleMediumFunction Instance { get; private set; }

        static SimpleMediumFunction()
        {
            Instance = new SimpleMediumFunction();
        }

        private SimpleMediumFunction()
        {
            
        }

        public bool CanReach(Position from, Position to)
        {
            return true;
        }
    }
}
