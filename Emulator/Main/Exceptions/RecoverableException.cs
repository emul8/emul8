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
    public class RecoverableException : Exception
    {
        public RecoverableException(Exception innerException) : base(String.Empty, innerException)
        {
        }

        public RecoverableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RecoverableException(string message):base(message)
        {
        }

        public RecoverableException():base()
        {
        }
    }
}

