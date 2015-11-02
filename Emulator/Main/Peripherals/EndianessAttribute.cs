//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using ELFSharp.ELF;

namespace Emul8.Peripherals
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EndianessAttribute : Attribute
    {
        public EndianessAttribute(Endianess endianess)
        {
            this.endianess = endianess;
        }

        public Endianess Endianess
        {
            get
            {
                return endianess;
            }
        }

        private readonly Endianess endianess;
    }
}

