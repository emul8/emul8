//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using Emul8.Utilities.Binding;
using System.Collections.Generic;
using System;
using Emul8.Logging;
using Emul8.Time;

namespace Emul8.Peripherals.CPU
{
    public partial class PowerPc : TranslationCPU, ICPUWithBlockBeginHook
    {
        public PowerPc(string cpuType, Machine machine, EndiannessEnum endianness = EndiannessEnum.BigEndian): base(cpuType, machine, endianness)
        {
            irqSync = new object();
            machine.ObtainClockSource().AddClockEntry(
                new ClockEntry(long.MaxValue/2, ClockEntry.FrequencyToRatio(this, 128000000), DecrementerHandler, false, Direction.Descending));
        }

        public override void OnGPIO(int number, bool value)
        {
            InternalSetInterrupt(InterruptType.External, value);
        }

        public override string Architecture { get { return "ppc"; } }

        public new void ClearHookAtBlockBegin()
        {
            base.ClearHookAtBlockBegin();
        }

        public new void SetHookAtBlockBegin(Action<uint, uint> hook)
        {
            base.SetHookAtBlockBegin(hook);
        }

        protected override Interrupt DecodeInterrupt(int number)
        {
            if(number == 0)
            {
                return Interrupt.Hard;
            }
            throw InvalidInterruptNumberException;
        }

        [Export]
        public uint ReadTbl()
        {
            tb += 0x100;
            return tb;
        }

        [Export]
        public uint ReadTbu()
        {
            return 0;
        }

        [Export]
        private uint ReadDecrementer()
        {
            return checked((uint)machine.ObtainClockSource().GetClockEntry(DecrementerHandler).Value);
        }

        [Export]
        private void WriteDecrementer(uint value)
        {
            machine.ObtainClockSource().ExchangeClockEntryWith(DecrementerHandler,
                entry => entry.With(period: value, value: value, enabled: value != 0));
        }

        private void InternalSetInterrupt(InterruptType interrupt, bool value)
        {
            lock(irqSync)
            {
                if(value)
                {
                    TlibSetPendingInterrupt((int)interrupt, 1);
                    base.OnGPIO(0, true);
                    return;
                }
                if(TlibSetPendingInterrupt((int)interrupt, 0) == 1)
                {
                    base.OnGPIO(0, false);
                }
            }
        }

        private void DecrementerHandler()
        {
            InternalSetInterrupt(InterruptType.Decrementer, true);
        }

        [Export]
        private void ResetInterruptEvent()
        {
            lock(irqSync)
            {
                ResetInterruptEvent(0);
            }
        }

        [Import]
        private FuncInt32Int32Int32 TlibSetPendingInterrupt;

        private uint tb;
        private readonly object irqSync;

        // have to be in sync with translation libs
        private enum InterruptType
        {
            Reset = 0,
            WakeUp,
            MachineCheck,
            External,
            SMI,
            CritictalExternal,
            Debug,
            Thermal,
            Decrementer,
            Hypervisor,
            PIT,
            FIT,
            WDT,
            CriticalDoorbell,
            Doorbell,
            PerformanceMonitor
        }
    }
}
