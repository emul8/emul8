//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;

namespace Emul8.Core.Structure.Registers
{
    /// <summary>
    /// Register field that provides a value of type T, where T is an enumeration.
    /// The maximum value of T must not exceed the field's width.
    /// </summary>
    public interface IEnumRegisterField<T> : IRegisterField<T> where T : struct, IConvertible
    {
    }
}
