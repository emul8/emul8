//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include <stdlib.h>
#include "emul8_imports.h"
#include "callbacks.h"

void (*on_translation_block_find_slow)(uint32_t pc);

void emul_attach_log_translation_block_fetch(void (handler)(uint32_t))
{
    on_translation_block_find_slow = handler;
}

void tlib_on_translation_block_find_slow(uint32_t pc)
{
  if(on_translation_block_find_slow)
  {
    (*on_translation_block_find_slow)(pc);
  }
}

EXTERNAL_AS(action_string, ReportAbort, tlib_abort)
EXTERNAL_AS(action_int32_string, LogAsCpu, tlib_log)

EXTERNAL_AS(func_uint32_uint32, ReadByteFromBus, tlib_read_byte)
EXTERNAL_AS(func_uint32_uint32, ReadWordFromBus, tlib_read_word)
EXTERNAL_AS(func_uint32_uint32, ReadDoubleWordFromBus, tlib_read_double_word)

EXTERNAL_AS(action_uint32_uint32, WriteByteToBus, tlib_write_byte)
EXTERNAL_AS(action_uint32_uint32, WriteWordToBus, tlib_write_word)
EXTERNAL_AS(action_uint32_uint32, WriteDoubleWordToBus, tlib_write_double_word)

EXTERNAL_AS(action_uint32_uint32, OnBlockBegin, tlib_on_block_begin)
EXTERNAL_AS(func_uint32, IsBlockBeginEventEnabled, tlib_is_block_begin_event_enabled)

EXTERNAL_AS(func_intptr_int32, Allocate, tlib_allocate)
void *tlib_malloc(size_t size)
{
  return tlib_allocate(size);
}
EXTERNAL_AS(func_intptr_intptr_int32, Reallocate, tlib_reallocate)
void *tlib_realloc(void *ptr, size_t size)
{
  return tlib_reallocate(ptr, size);
}
EXTERNAL_AS(action_intptr, Free, tlib_free)
EXTERNAL_AS(action_int32, OnTranslationCacheSizeChange, tlib_on_translation_cache_size_change)

EXTERNAL(action_intptr_intptr, invalidate_tb_in_other_cpus)
void tlib_invalidate_tb_in_other_cpus(unsigned long start, unsigned long end)
{
  invalidate_tb_in_other_cpus((void*)start, (void*)end);
}

EXTERNAL_AS(action_int32, UpdateInstructionCounter, update_instruction_counter_inner)
EXTERNAL_AS(func_uint32, IsInstructionCountEnabled, tlib_is_instruction_count_enabled)

static int32_t count_threshold;
static int32_t current_count_value;

void emul_set_count_threshold(int32_t value)
{
    count_threshold = value;
}

void tlib_update_instruction_counter(int32_t value)
{
  current_count_value += value;
  if(current_count_value < count_threshold)
  {
     return;
  }
  update_instruction_counter_inner(current_count_value);
  current_count_value = 0;
}

EXTERNAL_AS(func_int32, GetCpuIndex, tlib_get_cpu_index)
EXTERNAL_AS(action_uint32_uint32_uint32, LogDisassembly, tlib_on_block_translation)
