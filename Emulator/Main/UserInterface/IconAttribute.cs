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
    public class IconAttribute : Attribute
    {
        public IconAttribute(string resource)
        {
            ResourceName = resource;
        }

        public string ResourceName { get; private set; }
    }
}

