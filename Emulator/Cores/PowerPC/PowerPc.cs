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
using Emul8.Time;
using ELFSharp.ELF;
using Machine = Emul8.Core.Machine;
using ELFSharp.ELF.Sections;
using System.Linq;
using Emul8.Logging;
using ELFSharp.UImage;

namespace Emul8.Peripherals.CPU
{
    public partial class PowerPc : TranslationCPU, ICPUWithBlockBeginHook
    {
        public PowerPc(string cpuType, Machine machine, EndiannessEnum endianness = EndiannessEnum.BigEndian) : base(cpuType, machine, endianness)
        {
            irqSync = new object();
            machine.ObtainClockSource().AddClockEntry(
                new ClockEntry(long.MaxValue / 2, ClockEntry.FrequencyToRatio(this, 128000000), DecrementerHandler, false, Direction.Descending));
        }

        public override void InitFromUImage(UImage uImage)
        {
            this.Log(LogLevel.Warning, "PowerPC VLE mode not implemented for uImage loading.");
            base.InitFromUImage(uImage);
        }

        public override void InitFromElf(ELF<uint> elf)
        {
            var bamSection = elf.GetSections<Section<uint>>().FirstOrDefault(x => x.Name == ".__bam_bootarea");
            if(bamSection != null)
            {
                var bamSectionContents = bamSection.GetContents();
                var isValidResetConfigHalfWord = bamSectionContents[1] == 0x5a;
                if(!isValidResetConfigHalfWord)
                {
                    this.Log(LogLevel.Warning, "Invalid BAM section, ignoring.");
                }
                else
                {
                    StartInVle = (bamSectionContents[0] & 0x1) == 1;
                    this.Log(LogLevel.Info, "Will {0}start in VLE mode.", StartInVle ? "" : "not ");
                }
                base.InitFromElf(elf);
            }
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

        public bool StartInVle
        {
            get;
            set;
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

        [Export]
        private uint IsVleEnabled()
        {
            //this should present the current state. Now it's a stub only.
            return StartInVle ? 1u : 0u;
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
