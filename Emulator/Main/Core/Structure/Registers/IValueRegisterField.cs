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
    /// Register field that provides an arbitrary numeric value, not exceeding the register's width.
    /// </summary>
    public interface IValueRegisterField : IRegisterField<uint>
    {
    }
}

