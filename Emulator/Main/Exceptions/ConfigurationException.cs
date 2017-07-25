//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Exceptions
{
    public class ConfigurationException : System.Exception
    {
        public ConfigurationException(String message) : base(message)
        {
        }
    }
}
