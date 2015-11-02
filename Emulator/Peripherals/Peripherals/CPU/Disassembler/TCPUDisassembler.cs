//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Emul8.Utilities;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public class TCPUDisassembler : IDisassembler
    {
        public static bool IsAvailabale
        {
            get { return SharedLibraries.Exists("libdisas.so"); }
        }

        public TCPUDisassembler(IDisassemblable cpu) 
        {
            if (!mapping.ContainsKey(cpu.Architecture))
            {
                throw new ArgumentOutOfRangeException("arch");
            }

            Disassemble = cpu.Architecture == "arm-m" ? CortexMAddressTranslator.Wrap(mapping[cpu.Architecture]) : mapping[cpu.Architecture];
        }

        public static bool IsAvailableFor(string arch)
        {
            return mapping.ContainsKey(arch);
        }

        public DisassemblerType Type { get { return DisassemblerType.TCPU; } }
        public DisassemblyProvider Disassemble { get; private set; }

        private static readonly Dictionary<string, DisassemblyProvider> mapping = new Dictionary<string, DisassemblyProvider> 
        {
            { "i386",       X86        },
            { "arm",        Arm         },
            { "arm-m",      Arm         },
            { "ppc",        PowerPC     },
            { "sparc",      Sparc       }
        };

        [DllImport("libdisas.so", EntryPoint = "disas_x86")]
        protected static extern int X86(ulong pc, IntPtr memory, ulong size, uint flags, IntPtr output, ulong outputSize);   

        [DllImport("libdisas.so", EntryPoint = "disas_ARM")]
        protected static extern int Arm(ulong pc, IntPtr memory, ulong size, uint flags, IntPtr output, ulong outputSize);

        [DllImport("libdisas.so", EntryPoint = "disas_PPC")]
        protected static extern int PowerPC(ulong pc, IntPtr memory, ulong size, uint flags, IntPtr output, ulong outputSize);

        [DllImport("libdisas.so", EntryPoint = "disas_SPARC")]
        protected static extern int Sparc(ulong pc, IntPtr memory, ulong size, uint flags, IntPtr output, ulong outputSize);
    }
}

