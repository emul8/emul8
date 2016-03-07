//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.CPU;
using Emul8.Peripherals.Bus;
using Width = Emul8.Peripherals.Bus.Width;
using System.Collections.Generic;

namespace Emul8.Utilities.GDB.Commands
{
    internal class InsertBreakpointCommand : BreakpointCommandBase
    {
        public InsertBreakpointCommand(IControllableCPU cpu, SystemBus bus, WatchpointsContext context) : base(cpu, bus, context, "Z")
        {
        }

        protected override void Action(BreakpointType btype, uint address, uint kind)
        {
            switch(btype)
            {
            case BreakpointType.MemoryBreakpoint:
            case BreakpointType.HardwareBreakpoint:
                cpu.AddBreakpoint(address);
                break;
            case BreakpointType.AccessWatchpoint:
                AddWatchpointsCoveringMemoryArea(address, kind, Access.ReadAndWrite, true, watchpointsContext.AccessWatchpointHook);
                break;
            case BreakpointType.ReadWatchpoint:
                AddWatchpointsCoveringMemoryArea(address, kind, Access.Read, true, watchpointsContext.ReadWatchpointHook);
                break;
            case BreakpointType.WriteWatchpoint:
                AddWatchpointsCoveringMemoryArea(address, kind, Access.Write, true, watchpointsContext.WriteWatchpointHook);
                break;
            default:
                throw new ArgumentException("Breakpoint type not supported");
            }
        }

        private void AddWatchpointsCoveringMemoryArea(long address, uint kind, Access access, bool updateContext, Action<long, Width> hook)
        {
            // we need to register hooks for all possible access widths convering memory fragment
            // [address, address + kind) referred by GDB
            foreach(var range in CalculateAllCoveringAddressess(address, kind))
            {
                var descriptor = new WatchpointsContext.WatchpointDescriptor
                {
                    Address = range.Item2,
                    Width = range.Item1,
                    Access = access,
                    UpdateContext = updateContext,
                    Hook = hook
                };

                if(watchpointsContext.AddWatchpoint(descriptor))
                {
                    systemBus.AddWatchpointHook(range.Item2, range.Item1, access, updateContext, hook);
                }
            }
        }
    }

    internal class RemoveBreakpointCommand : BreakpointCommandBase
    {
        public RemoveBreakpointCommand(IControllableCPU cpu, SystemBus bus, WatchpointsContext context) : base(cpu, bus, context, "z")
        {
        }

        protected override void Action(BreakpointType btype, uint address, uint kind)
        {
            switch(btype)
            {
            case BreakpointType.MemoryBreakpoint:
            case BreakpointType.HardwareBreakpoint:
                cpu.RemoveBreakpoint(address);
                break;
            case BreakpointType.AccessWatchpoint:
                RemoveWatchpointsCoveringMemoryArea(address, kind, Access.ReadAndWrite, true, watchpointsContext.AccessWatchpointHook);
                break;
            case BreakpointType.ReadWatchpoint:
                RemoveWatchpointsCoveringMemoryArea(address, kind, Access.Read, true, watchpointsContext.ReadWatchpointHook);
                break;
            case BreakpointType.WriteWatchpoint:
                RemoveWatchpointsCoveringMemoryArea(address, kind, Access.Write, true, watchpointsContext.WriteWatchpointHook);
                break;
            default:
                throw new ArgumentException("Breakpoint type not supported");
            }
        }

        private void RemoveWatchpointsCoveringMemoryArea(long address, uint kind, Access access, bool updateContext, Action<long, Width> hook)
        {
            // we need to register hooks for all possible access widths convering memory fragment 
            // [address, address + kind) referred by GDB
            foreach(var range in CalculateAllCoveringAddressess(address, kind))
            {
                var descriptor = new WatchpointsContext.WatchpointDescriptor
                {
                    Address = range.Item2,
                    Width = range.Item1,
                    Access = access,
                    UpdateContext = updateContext,
                    Hook = hook
                };

                if(watchpointsContext.RemoveWatchpoint(descriptor))
                {
                    systemBus.RemoveWatchpointHook(range.Item2, hook);
                }
            }
        }
    }

    internal abstract class BreakpointCommandBase : Command
    {
        protected BreakpointCommandBase(IControllableCPU cpu, SystemBus bus, WatchpointsContext context, string mnemonic) : base(mnemonic)
        {
            this.cpu = cpu;
            systemBus = bus;
            watchpointsContext = context;
        }

        protected static IEnumerable<Tuple<Width, long>> CalculateAllCoveringAddressess(long address, uint kind)
        {
            foreach(Width width in Enum.GetValues(typeof(Width)))
            {
                for(var offset = -(address % (int)width); offset < kind; offset += (int)width)
                {
                    yield return Tuple.Create(width, address + offset);
                }
            }
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var splittedArguments = GetCommandArguments(packet.Data, Separators, 3);
            if(splittedArguments.Length < 3)
            {
                throw new ArgumentException("Expected at least 3 arguments");
            }

            int typeAsInt;
            if(!int.TryParse(splittedArguments[0], out typeAsInt) || typeAsInt < 0 || typeAsInt > 4)
            {
                throw new ArgumentException("Could not parse breakpoint type");
            }
            var type = (BreakpointType)typeAsInt;

            uint address;
            if(!uint.TryParse(splittedArguments[1], System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                throw new ArgumentException("Could not parse address");
            }

            uint kind;
            if(!uint.TryParse(splittedArguments[2], System.Globalization.NumberStyles.HexNumber, null, out kind))
            {
                throw new ArgumentException("Could not parse kind");
            }

            // we do not support cond_list yet!
            Action(type, address, kind);

            return PacketData.Success;
        }

        protected abstract void Action(BreakpointType btype, uint address, uint kind);

        protected readonly IControllableCPU cpu;
        protected readonly SystemBus systemBus;
        protected readonly WatchpointsContext watchpointsContext;

        private static readonly char[] Separators = { ',', ';' };
    }

    internal class WatchpointsContext
    {
        public WatchpointsContext(IControllableCPU cpu)
        {
            var translationCpu = cpu as TranslationCPU;
            if(translationCpu == null)
            {
                throw new ArgumentException("Watchpoints are supported on TranslationCPU only");
            }

            this.cpu = translationCpu;
            watchpoints = new Dictionary<WatchpointDescriptor, int>();
        }

        public bool AddWatchpoint(WatchpointDescriptor descriptor)
        {
            lock(watchpoints)
            {
                if(watchpoints.ContainsKey(descriptor))
                {
                    watchpoints[descriptor]++;
                    return false;
                }

                watchpoints.Add(descriptor, 1);
                return true;
            }
        }

        public bool RemoveWatchpoint(WatchpointDescriptor descriptor)
        {
            lock(watchpoints)
            {
                if(watchpoints[descriptor] > 1)
                {
                    watchpoints[descriptor]--;
                    return false;
                }

                watchpoints.Remove(descriptor);
                return true;
            }
        }

        public void AccessWatchpointHook(long address, Width width)
        {
            cpu.HaltOnWatchpoint(new HaltArguments(HaltReason.Breakpoint, checked((uint)address), BreakpointType.AccessWatchpoint));
        }

        public void WriteWatchpointHook(long address, Width width)
        {
            cpu.HaltOnWatchpoint(new HaltArguments(HaltReason.Breakpoint, checked((uint)address), BreakpointType.WriteWatchpoint));
        }

        public void ReadWatchpointHook(long address, Width width)
        {
            cpu.HaltOnWatchpoint(new HaltArguments(HaltReason.Breakpoint, checked((uint)address), BreakpointType.ReadWatchpoint));
        }

        private readonly TranslationCPU cpu;
        private readonly Dictionary<WatchpointDescriptor, int> watchpoints;

        public class WatchpointDescriptor
        {
            public long Address;
            public Width Width;
            public Access Access;
            public bool UpdateContext;
            public Action<long, Width> Hook;

            public override bool Equals(object obj)
            {
                var objAsBreakpointDescriptor = obj as WatchpointDescriptor;
                if(objAsBreakpointDescriptor == null)
                {
                    return false;
                }

                return objAsBreakpointDescriptor.Address == Address
                        && objAsBreakpointDescriptor.Width == Width
                        && objAsBreakpointDescriptor.Access == Access
                        && objAsBreakpointDescriptor.UpdateContext == UpdateContext
                        && objAsBreakpointDescriptor.Hook == Hook;
            }

            public override int GetHashCode()
            {
                return 17 * (int)Address 
                    + 23 * (int)Width 
                    + 17 * (int)Access 
                    + (UpdateContext ? 0 : 23) 
                    + 17 * Hook.GetHashCode();
            }
        }
    }
}

