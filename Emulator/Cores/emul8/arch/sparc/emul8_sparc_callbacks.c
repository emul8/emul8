//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include "arch_callbacks.h"
#include "emul8_imports.h"

EXTERNAL_AS(func_int32, FindBestInterrupt, tlib_find_best_interrupt)
EXTERNAL_AS(action_int32, AcknowledgeInterrupt, tlib_acknowledge_interrupt)
EXTERNAL_AS(action, OnCpuHalted, tlib_on_cpu_halted)
EXTERNAL_AS(action, OnCpuPowerDown, tlib_on_cpu_power_down)
