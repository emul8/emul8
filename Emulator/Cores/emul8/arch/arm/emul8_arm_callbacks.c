//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include "arch_callbacks.h"
#include "emul8_imports.h"

#ifdef TARGET_PROTO_ARM_M
EXTERNAL_AS(func_int32, AcknowledgeIRQ, tlib_nvic_acknowledge_irq)
EXTERNAL_AS(action_int32, CompleteIRQ, tlib_nvic_complete_irq)
EXTERNAL_AS(action_int32, SetPendingIRQ, tlib_nvic_set_pending_irq)
EXTERNAL_AS(action_int32, OnPRIMASKWrite, tlib_nvic_write_primask)
EXTERNAL_AS(action_int32, OnBASEPRIWrite, tlib_nvic_write_basepri)
EXTERNAL_AS(func_int32, PendingMaskedIRQ, tlib_nvic_get_pending_masked_irq)
#endif

EXTERNAL_AS(func_uint32_uint32, Read32CP15, tlib_read_cp15_32)
EXTERNAL_AS(action_uint32_uint32, Write32CP15, tlib_write_cp15_32)
EXTERNAL_AS(func_uint64_uint32, Read64CP15, tlib_read_cp15_64)
EXTERNAL_AS(action_uint32_uint64, Write64CP15, tlib_write_cp15_64)
EXTERNAL_AS(func_uint32, IsWfiAsNop, tlib_is_wfi_as_nop)
EXTERNAL_AS(func_uint32, DoSemihosting, tlib_do_semihosting)
