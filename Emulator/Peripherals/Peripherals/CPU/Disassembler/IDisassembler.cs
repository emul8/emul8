//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.CPU.Disassembler;
using System.Reflection;
using System.Linq;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public delegate int DisassemblyProvider(UInt64 pc, IntPtr memory, UInt64 size, UInt32 flags, IntPtr output, UInt64 outputSize);

    public interface IDisassembler
    {
        DisassemblyProvider Disassemble { get; }
        string Name { get; }
    }
}

