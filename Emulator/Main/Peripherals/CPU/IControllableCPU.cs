//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using ELFSharp.ELF;
using ELFSharp.UImage;

namespace Emul8.Peripherals.CPU
{
    public interface IControllableCPU : ICPU
    {
        void SetRegisterUnsafe(int register, uint value);

        uint GetRegisterUnsafe(int register);

        string[,] GetRegistersValues();

        void SetSingleStepMode(bool on);

        void WaitForStepDone();

        bool InSingleStep();

        void SingleStep();

        void AddBreakpoint(uint addr);

        void RemoveBreakpoint(uint addr);

        void InitFromElf(ELF<uint> elf);

        void InitFromUImage(UImage uImage);

        event Action<HaltReason> Halted;
    }
}

