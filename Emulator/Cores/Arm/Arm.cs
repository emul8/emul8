//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Utilities.Binding;
using System.Collections.Generic;
using Emul8.Time;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.UART;
using Emul8.Core.Structure;
using Emul8.Exceptions;
using Emul8.Utilities;
using Emul8.Peripherals.Miscellaneous;

namespace Emul8.Peripherals.CPU
{
    public partial class Arm : TranslationCPU, IClockSource, ICPUWithBlockBeginHook, IPeripheralRegister<SemihostingUart, NullRegistrationPoint>
    {
        public Arm(string cpuType, Machine machine, EndiannessEnum endianness = EndiannessEnum.LittleEndian): base(cpuType, machine, endianness)
        {
        }
            
        public void Register(SemihostingUart peripheral, NullRegistrationPoint registrationPoint)
        {
            if(semihostingUart != null)
            {
                throw new RegistrationException("A semihosting uart is already registered.");
            }
            semihostingUart = peripheral;
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Unregister(SemihostingUart peripheral)
        {
            semihostingUart = null;
            machine.UnregisterAsAChildOf(this, peripheral);
        }

        public override string Architecture { get { return "arm"; } }

        void IClockSource.ExecuteInLock(Action action)
        {
            ClockSource.ExecuteInLock(action);
        }

        void IClockSource.AddClockEntry(ClockEntry entry)
        {
            ClockSource.AddClockEntry(entry);
        }

        void IClockSource.ExchangeClockEntryWith(Action handler, Func<ClockEntry, ClockEntry> visitor,
            Func<ClockEntry> factorIfNonExistant)
        {
            ClockSource.ExchangeClockEntryWith(handler, visitor, factorIfNonExistant);
        }

        ClockEntry IClockSource.GetClockEntry(Action handler)
        {
            return ClockSource.GetClockEntry(handler);
        }

        void IClockSource.GetClockEntryInLockContext(Action handler, Action<ClockEntry> visitor)
        {
            ClockSource.GetClockEntryInLockContext(handler, visitor);
        }

        IEnumerable<ClockEntry> IClockSource.GetAllClockEntries()
        {
            return ClockSource.GetAllClockEntries();
        }

        bool IClockSource.RemoveClockEntry(Action handler)
        {
            return ClockSource.RemoveClockEntry(handler);
        }

        long IClockSource.CurrentValue
        {
            get
            {
                return ClockSource.CurrentValue;
            }
        }

        IEnumerable<ClockEntry> IClockSource.EjectClockEntries()
        {
            return ClockSource.EjectClockEntries();
        }

        void IClockSource.AddClockEntries(IEnumerable<ClockEntry> entries)
        {
            ClockSource.AddClockEntries(entries);
        }

        public uint ID
        {
            get
            {
                return TlibGetCpuId();
            }
            set
            {
                TlibSetCpuId(value);
            }
        }

        public new void ClearHookAtBlockBegin()
        {
            base.ClearHookAtBlockBegin();
        }

        public new void SetHookAtBlockBegin(Action<uint, uint> hook)
        {
            base.SetHookAtBlockBegin(hook);
        }

        public bool WfiAsNop { get; set; }

        [Export]
        protected uint Read32CP15(uint instruction)
        {
            return Read32CP15Inner(instruction);
        }

        [Export]
        protected void Write32CP15(uint instruction, uint value)
        {
            Write32CP15Inner(instruction, value);
        }

        [Export]
        protected ulong Read64CP15(uint instruction)
        {
            return Read64CP15Inner(instruction);
        }

        [Export]
        protected void Write64CP15(uint instruction, ulong value)
        {
            Write64CP15Inner(instruction, value);
        }

        protected override Interrupt DecodeInterrupt(int number)
        {
            switch(number)
            {
            case 0:
                return Interrupt.Hard;
            case 1:
                return Interrupt.TargetExternal1;
            default:
                throw InvalidInterruptNumberException;
            }
        }

        protected virtual uint Read32CP15Inner(uint instruction)
        {
            uint op1, op2, crm, crn;
            crm = instruction & 0xf;
            crn = (instruction >> 16) & 0xf;
            op1 = (instruction >> 21) & 7;
            op2 = (instruction >> 5) & 7;

            if((op1 == 4) && (op2 == 0) && (crm == 0))
            {
                // scu
                var scus = machine.GetPeripheralsOfType<SnoopControlUnit>().ToArray();
                switch(scus.Length)
                {
                case 0:
                    this.Log(LogLevel.Warning, "Trying to read SCU address, but SCU was not found - returning 0x0.");
                    return 0;
                case 1:
                    return (uint)((BusRangeRegistration)(machine.GetPeripheralRegistrationPoints(machine.SystemBus, scus[0]).Single())).Range.StartAddress;
                default:
                    this.Log(LogLevel.Error, "Trying to read SCU address, but more than one instance was found. Aborting.");
                    throw new CpuAbortException();
                }
            }
            this.Log(LogLevel.Warning, "Unknown CP15 32-bit read - op1={0}, op2={1}, crm={2}, crn={3} - returning 0x0", op1, op2, crm, crn);
            return 0;
        }

        protected virtual void Write32CP15Inner(uint instruction, uint value)
        {
            uint op1, op2, crm, crn;
            crm = instruction & 0xf;
            crn = (instruction >> 16) & 0xf;
            op1 = (instruction >> 21) & 7;
            op2 = (instruction >> 5) & 7;

            this.Log(LogLevel.Warning, "Unknown CP15 32-bit write - op1={0}, op2={1}, crm={2}, crn={3}", op1, op2, crm, crn);
        }

        protected virtual ulong Read64CP15Inner(uint instruction)
        {
            uint op1, crm;
            crm = instruction & 0xf;
            op1 = (instruction >> 4) & 0xf;
            this.Log(LogLevel.Warning, "Unknown CP15 64-bit read - op1={0}, crm={1} - returning 0x0",op1, crm);
            return 0;
        }

        protected virtual void Write64CP15Inner(uint instruction, ulong value)
        {
            uint op1, crm;
            crm = instruction & 0xf;
            op1 = (instruction >> 4) & 0xf;
            this.Log(LogLevel.Warning, "Unknown CP15 64-bit write - op1={0}, crm={1}", op1, crm);
        }

    	[Export]
    	private uint DoSemihosting() {
            var uart = semihostingUart;
            //this.Log(LogLevel.Error, "Semihosing, r0={0:X}, r1={1:X} ({2:X})", this.GetRegisterUnsafe(0), this.GetRegisterUnsafe(1), this.TranslateAddress(this.GetRegisterUnsafe(1)));

    	    uint operation = R[0];
    	    uint r1 = R[1];
    	    uint result = 0;
    	    switch (operation) {
    	    	case 7: // SYS_READC
    		    if (uart == null) break;
    	            result = uart.SemihostingGetByte();
    		    break;
    	        case 3: // SYS_WRITEC
    	    	case 4: // SYS_WRITE0
    		    if (uart == null) break;
    		    string s = "";
    		    uint addr = this.TranslateAddress(r1);
    		    do {
    		    	var c = this.Bus.ReadByte(addr++);
    			if (c == 0) break;
    			s = s + Convert.ToChar(c);
    			if ((operation) == 3) break; // SYS_WRITEC
    		    } while (true);
    	            uart.SemihostingWriteString(s);
    		    break;
    		default:
    		    this.Log(LogLevel.Debug, "Unknown semihosting operation: 0x{0:X}", operation);
    		    break;
    	     }
    	     return result;
    	}

        [Export]
        private uint IsWfiAsNop()
        {
            return WfiAsNop ? 1u : 0u;
        }
         
        private UInt32 BeforePCWrite(UInt32 value)
        {
            SetThumb((int)(value & 0x1));
            return value & ~(uint)0x1;
        }

        private SemihostingUart semihostingUart = null;

        [Import]
        private ActionUInt32 TlibSetCpuId;

        [Import]
        private FuncUInt32 TlibGetCpuId;

        [Import]
        private ActionInt32 SetThumb;
    }
}

