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
using Emul8.Utilities;
using System.Reflection;

namespace Emul8.Peripherals.CPU.Disassembler
{
    public class DisassemblerManager
    {
        static DisassemblerManager()
        {
            Instance = new DisassemblerManager();
        }

        public static DisassemblerManager Instance { get; private set; }

        public string[] GetAvailableDisassemblers(string cpuArchitecture = null)
        {
            var disassemblers = TypeManager.Instance.AutoLoadedTypes.Where(x => x.GetCustomAttribute<DisassemblerAttribute>() != null);
            return disassemblers
                        .Where(x => (cpuArchitecture == null) || 
                                x.GetCustomAttribute<DisassemblerAttribute>().Architectures.Contains(cpuArchitecture))
                        .Select(x => x.GetCustomAttribute<DisassemblerAttribute>().Name).ToArray();
        }

        public IDisassembler CreateDisassembler(string type, IDisassemblable cpu)
        {
            var disassemblerType = TypeManager.Instance.AutoLoadedTypes.Where(x => x.GetCustomAttribute<DisassemblerAttribute>() != null 
                && x.GetCustomAttribute<DisassemblerAttribute>().Name == type
                && x.GetCustomAttribute<DisassemblerAttribute>().Architectures.Contains(cpu.Architecture)).SingleOrDefault();
 
            return disassemblerType == null
                ? null
                : (IDisassembler)disassemblerType.GetConstructor(new [] { typeof(IDisassemblable) }).Invoke(new [] { cpu });
        }

        private DisassemblerManager()
        {
        }
    }
}

