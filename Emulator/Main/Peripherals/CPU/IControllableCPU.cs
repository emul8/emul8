//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using ELFSharp.ELF;
using ELFSharp.UImage;

namespace Emul8.Peripherals.CPU
{
    public interface IControllableCPU : ICPU
    {
        void SetRegisterUnsafe(int register, uint value);

        uint GetRegisterUnsafe(int register);

        int[] GetRegisters();

        string[,] GetRegistersValues();

        void InitFromElf(ELF<uint> elf);

        void InitFromUImage(UImage uImage);

        Endianess Endianness { get; }
    }
}

