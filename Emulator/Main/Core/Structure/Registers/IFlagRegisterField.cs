//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Core.Structure.Registers
{
    /// <summary>
    /// Register field that provides a boolean value. It's width is always equal to 1.
    /// </summary>
    public interface IFlagRegisterField : IRegisterField<bool>
    {
    }
}

