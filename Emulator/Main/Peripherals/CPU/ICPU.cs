//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Utilities;

namespace Emul8.Peripherals.CPU
{
    public interface ICPU : IPeripheral, IHasOwnLife
    {       
        void MapMemory(IMappedSegment segment);
        void UnmapMemory(Range range);
        string Model{ get; }
        uint PC { get; set; }
        bool IsHalted { get; set; }
        SystemBus Bus { get; }
        void UpdateContext();
        /// <summary>
        /// Returns true if the thread calling this property is possesed
        /// by the object.
        /// </summary>
        bool OnPossessedThread { get; }
    }

    public static class ICPUExtensions
    {
        public static string GetCPUThreadName(this ICPU cpu, Machine machine)
        {
            string machineName;
            if(EmulationManager.Instance.CurrentEmulation.TryGetMachineName(machine, out machineName))
            {
                machineName += ".";
            }
            return "{0}{1}[{2}]".FormatWith(machineName, machine.GetLocalName(cpu), machine.SystemBus.GetCPUId(cpu));
        }
    }
}

