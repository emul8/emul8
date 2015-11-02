//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Utilities;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public class LLVMDisassembler : IDisassembler
    {
        public static bool IsAvailable
        {
            get { return SharedLibraries.Exists("libLLVM.so"); }
        }

        public static bool IsAvailableFor(string arch)
        {
            return SupportedArchitectures.ContainsKey(arch);
        } 

        public LLVMDisassembler(IDisassemblable cpu) 
        {
            if (!SupportedArchitectures.ContainsKey(cpu.Architecture))
            {
                throw new ArgumentOutOfRangeException("cpu");
            }

            this.cpu = cpu;
            cache = new Dictionary<string, LLVMDisasWrapper>();

            Disassemble = cpu.Architecture == "arm-m" ? CortexMAddressTranslator.Wrap(LLVMDisassemble) : LLVMDisassemble;
        }

        public DisassemblerType Type { get { return DisassemblerType.LLVM; } }
        public DisassemblyProvider Disassemble { get; private set; }

        private int LLVMDisassemble(ulong pc, IntPtr memory, ulong size, uint flags, IntPtr output, ulong outputSize)
        {
            var triple = SupportedArchitectures[cpu.Architecture];
            if (triple == "armv7a" && flags > 0)
            {
                triple = "thumb";
            }

            var key = string.Format("{0} {1}", triple, cpu.Model);
            if (!cache.ContainsKey(key))
            {
                string model;
                switch(cpu.Model)
                {
                case "x86":
                    model = "i386";
                    break;
                // this case is included because of #3250
                case "arm926":
                    model = "arm926ej-s";
                    break;
                default:
                    model = cpu.Model;
                    break;
                }

                cache.Add(key, new LLVMDisasWrapper(model, triple));
            }

            return cache[key].Disassemble(memory, (ulong)size, pc, output, (uint)outputSize);
        }

        private static readonly Dictionary<string, string> SupportedArchitectures = new Dictionary<string, string>
        {
            { "arm",    "armv7a"    },
            { "arm-m",  "armv7a"    },
            { "mips",   "mipsel"    },
            { "i386",   "i386"      }
        };

        private readonly Dictionary<string, LLVMDisasWrapper> cache;
        private readonly IDisassemblable cpu;
    }
}
