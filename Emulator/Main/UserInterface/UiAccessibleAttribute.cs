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
    public class UiAccessibleAttribute : Attribute
    {
        public string FriendlyName { get; set; }
    }
}

