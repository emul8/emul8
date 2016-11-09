//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.CPU.Disassembler
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisassemblerAttribute : Attribute
    {
        public DisassemblerAttribute(string name, string[] architectures)
        {
            Name = name;
            Architectures = architectures;
        }

        public string Name { get; private set; }
        public string[] Architectures { get; private set; }
    }
}

