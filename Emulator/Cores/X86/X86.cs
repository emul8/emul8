//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using Emul8.Time;
using System.Collections.Generic;
using System;

namespace Emul8.Peripherals.CPU
{
    public partial class X86 : TranslationCPU
    {
        const EndiannessEnum endianness = EndiannessEnum.LittleEndian;

        public X86(string cpuType, Machine machine): base(cpuType, machine, endianness) { }

        public override string Architecture { get { return "i386"; } }

        protected override Interrupt DecodeInterrupt(int number)
        {
            if(number == 0)
            {
                return Interrupt.Hard;
            }
            throw InvalidInterruptNumberException;
        }
    }
}

