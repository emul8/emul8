//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include <stdlib.h>
#include "cpu.h"
#include "emul8_imports.h"
#include "callbacks.h"

extern CPUState *cpu;

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

EXTERNAL_AS(func_int32_uint32, IsIoAccessed, tlib_is_io_accessed)

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

void emul_set_count_threshold(int32_t value)
{
  cpu->instructions_count_threshold = value;
}

void tlib_update_instruction_counter(int32_t value)
{
  cpu->instructions_count_value += value;
  if(cpu->instructions_count_value < cpu->instructions_count_threshold)
  {
     return;
  }
  update_instruction_counter_inner(cpu->instructions_count_value);
  cpu->instructions_count_value = 0;
}

EXTERNAL_AS(func_int32, GetCpuIndex, tlib_get_cpu_index)
EXTERNAL_AS(action_uint32_uint32_uint32, LogDisassembly, tlib_on_block_translation)
