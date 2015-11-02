//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include "arch_callbacks.h"
#include "emul8_imports.h"

EXTERNAL_AS(func_uint32, ReadTbl, tlib_read_tbl)
EXTERNAL_AS(func_uint32, ReadTbu, tlib_read_tbu)
EXTERNAL_AS(func_uint32, ReadDecrementer, tlib_read_decrementer)
EXTERNAL_AS(action_uint32, WriteDecrementer, tlib_write_decrementer)
EXTERNAL_AS(action, ResetInterruptEvent, tlib_on_interrupt_complete)
