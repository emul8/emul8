//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.IRQControllers;
using Emul8.Utilities.Binding;
using Emul8.Logging;
using Antmicro.Migrant.Hooks;
using Emul8.Exceptions;

namespace Emul8.Peripherals.CPU
{
    public partial class CortexM : Arm, IControllableCPU
    {
        public CortexM(string cpuType, Machine machine, NVIC nvic, EndiannessEnum endianness = EndiannessEnum.LittleEndian) : base(cpuType, machine, endianness)
        {
            if(nvic == null)
            {
                throw new RecoverableException(new ArgumentNullException("nvic"));
            }

            this.nvic = nvic;
            nvic.AttachCPU(this);
            Init();
        }

        public override void Start()
        {
            InitPCAndSP();
            base.Start();
        }

        public override void Reset()
        {
            pcNotInitialized = true;
            vtorInitialized = false;
            base.Reset();
        }

        public override void Resume()
        {
            InitPCAndSP();
            base.Resume();
        }

        [LatePostDeserialization]
        private void Init()
        {
            ExtendWaitHandlers(nvic.MaskedInterruptPresent);
        }

        public override string Architecture { get { return "arm-m"; } }

        public uint VectorTableOffset
        {
            get
            {
                return tlibGetInterruptVectorBase();
            }
            set
            {
                vtorInitialized = true;
                if(machine.SystemBus.FindMemory(value) == null)
                {
                    this.Log(LogLevel.Warning, "Tried to set VTOR address at 0x{0:X} which does not lay in memory. Aborted.", value);
                    return;
                }
                this.NoisyLog("VectorTableOffset set to 0x{0:X}.", value);
                tlibSetInterruptVectorBase(value);
            }
        }

        public bool FpuEnabled
        {
            set
            {
                tlibToggleFpu(value ? 1 : 0);
            }
        }

        void IControllableCPU.InitWithEntryPoint(uint value)
        {
            // do nothing
        }

        private void InitPCAndSP()
        {
            if(!vtorInitialized && machine.SystemBus.LowestLoadedAddress.HasValue)
            {
                var value = machine.SystemBus.LowestLoadedAddress.Value;
                this.Log(LogLevel.Info, "Guessing VectorTableOffset value to be 0x{0:X}.", value);
                VectorTableOffset = value;
            }
            if(pcNotInitialized)
            {
                pcNotInitialized = false;
                // stack pointer and program counter are being sent according
                // to VTOR (vector table offset register)
                var sysbus = machine.SystemBus;
                var pc = sysbus.ReadDoubleWord(VectorTableOffset + 4);
                var sp = sysbus.ReadDoubleWord(VectorTableOffset);
                if(sysbus.FindMemory(pc) == null || (pc == 0 && sp == 0))
                {
                    this.Log(LogLevel.Error, "PC does not lay in memory or PC and SP are equal to zero. CPU was halted.");
                    IsHalted = true;
                }
                this.Log(LogLevel.Info, "Setting initial values: PC = 0x{0:X}, SP = 0x{1:X}", pc, sp);
                PC = pc;
                SP = sp;
            }
        }

        [Export]
        private void SetPendingIRQ(int number)
        {
            nvic.SetPendingIRQ(number);
        }

        [Export]
        private int AcknowledgeIRQ()
        {
            var result = nvic.AcknowledgeIRQ();
            return result;
        }

        [Export]
        private void CompleteIRQ(int number)
        {
            nvic.CompleteIRQ(number);
        }

        [Export]
        private void OnBASEPRIWrite(int value)
        {
            nvic.BASEPRI = (byte)value;
        }

        [Export]
        private void OnPRIMASKWrite(int value)
        {
            if (nvic != null)
            {
                nvic.PRIMASK = (value != 0);
            }
        }

        [Export]
        private int PendingMaskedIRQ()
        {
            return nvic.MaskedInterruptPresent.WaitOne(0) ? 1 : 0;
        }


        private NVIC nvic;
        private bool pcNotInitialized = true;
        private bool vtorInitialized;

        [Import]
        private ActionInt32 tlibToggleFpu;

        [Import]
        private FuncUInt32 tlibGetInterruptVectorBase;

        [Import]
        private ActionUInt32 tlibSetInterruptVectorBase;
    }
}

