//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.CPU
{
    public interface ICpuSupportingGdb : ICPUWithHooks, IControllableCPU
    {
        void Step(int count = 1, bool wait = true);
        ExecutionMode ExecutionMode { get; set; }
        event Action<HaltArguments> Halted;
        void EnterSingleStepModeSafely(HaltArguments args);

        void StartGdbServer(int port);
        void StopGdbServer();
        bool IsGdbServerCreated { get; }
    }
}

