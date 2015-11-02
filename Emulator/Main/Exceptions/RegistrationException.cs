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
    public class RegistrationException : RecoverableException
    {

        public RegistrationException(string name, string parentName):base(string.Format("Could not register {0} in {1}.", name, parentName))
        {
        }

        public RegistrationException (string name, string parentName, string reason):base(string.Format("Could not register {0} in {1}. Reason: {2}.", name, parentName, reason))
        {
        }
        public RegistrationException(string message):base(message)
        {
        }
    }
}

