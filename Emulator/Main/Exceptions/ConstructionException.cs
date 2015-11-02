//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Exceptions
{
    public class ConstructionException : RecoverableException
    {
        public ConstructionException (string message):base(message)
        {
        }

        public ConstructionException (string message, Exception innerException):base(message, innerException)
        {
        }
    }
}

