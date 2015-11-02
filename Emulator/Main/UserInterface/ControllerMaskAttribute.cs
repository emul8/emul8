//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.UserInterface
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ControllerMaskAttribute : Attribute
    {
        public ControllerMaskAttribute(params Type[] types)
        {
            MaskedTypes = types;
        }

        public Type[] MaskedTypes { get; private set; }
    }
}

