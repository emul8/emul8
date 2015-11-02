//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.Bus;
using Emul8.Core;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public interface IDisassemblable
    {
        SystemBus Bus { get; }
        Symbol SymbolLookup(uint addr);
        bool LogTranslatedBlocks { get; set; }

        string Architecture { get; }
        string Model { get; }
    }
}

