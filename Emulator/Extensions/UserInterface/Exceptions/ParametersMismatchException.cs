//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Exceptions;

namespace Emul8.UserInterface.Exceptions
{
    public class ParametersMismatchException : RecoverableException
    {
        public ParametersMismatchException() : base("Parameters did not match the signature")
        {
        }
        public ParametersMismatchException(string message) : base(message)
        {
        }
    }
}
