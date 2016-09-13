//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Robot
{
    public class KeywordException : Exception
    {
        public KeywordException(string message, params object[] args) : base(string.Format(message, args))
        {
        }
    }
}

