//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;

namespace Emul8.Core.Structure
{
    /// <summary>
    /// Point under which a peripheral can be registered.
    /// <remarks>
    /// Not every registration point type can be used for addressing the peripheral.
    /// Some peripherals allow registering via more than one type of registration point,
    /// which interally get converted to a type used for addressing and retrieval of registered
    /// peripherals.
    /// <remarks>
    /// </summary>
    public interface IRegistrationPoint : IInterestingType
    {
        string PrettyString { get; }
    }
}

