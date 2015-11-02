//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public static class DisassemblerManager
    {
        public static DisassemblerType[] AvailableDisassemblers 
        { 
            get { return RegisteredDisassemblers.Where(d => d.Value.IsAvailable()).Select(d => d.Key).ToArray(); }
        }

        public static DisassemblerType[] GetAvailableDisassemblersForArchitecture(string arch)
        {
            return RegisteredDisassemblers.Where(d => d.Value.IsAvailable() && d.Value.IsAvailableForArchitecture(arch)).Select(d => d.Key).ToArray();
        }

        public static IDisassembler CreateDisassembler(DisassemblerType type, IDisassemblable cpu)
        {
            var disas = RegisteredDisassemblers[type];
            return !(disas.IsAvailable() && disas.IsAvailableForArchitecture(cpu.Architecture)) ? null : RegisteredDisassemblers[type].Construct(cpu);
        }

        private static readonly Dictionary<DisassemblerType, DisassemblerDescriptor> RegisteredDisassemblers = new Dictionary<DisassemblerType, DisassemblerDescriptor> 
        {
            { DisassemblerType.LLVM, new DisassemblerDescriptor { IsAvailable = () => LLVMDisassembler.IsAvailable, IsAvailableForArchitecture = LLVMDisassembler.IsAvailableFor, Construct = cpu => new LLVMDisassembler(cpu) } },
            { DisassemblerType.TCPU, new DisassemblerDescriptor { IsAvailable = () => TCPUDisassembler.IsAvailabale, IsAvailableForArchitecture = TCPUDisassembler.IsAvailableFor, Construct = cpu => new TCPUDisassembler(cpu) } }
        };

        private struct DisassemblerDescriptor
        {
            public Func<bool> IsAvailable;
            public Func<string, bool> IsAvailableForArchitecture;
            public Func<IDisassemblable, IDisassembler> Construct;
        }
    }
}

