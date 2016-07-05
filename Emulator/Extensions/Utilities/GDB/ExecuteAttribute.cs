//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Utilities.GDB
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ExecuteAttribute : Attribute
    {
        public ExecuteAttribute(string mnemonic)
        {
            Mnemonic = mnemonic;
        }

        public string Mnemonic { get; private set; }
    }
}

