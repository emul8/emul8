//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Bootstrap
{
    public interface IStep
    {
        IStepResult Run(StepManager manager);
    }
}

