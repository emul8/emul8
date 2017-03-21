//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

#include <stdint.h>
#include "arch_callbacks.h"
#include "emul8_imports.h"

EXTERNAL(func_uint32_uint32, read_byte_from_port)
uint8_t tlib_read_byte_from_port(uint16_t address)
{
  return (uint8_t)read_byte_from_port(address);
}

EXTERNAL(func_uint32_uint32, read_word_from_port)
uint16_t tlib_read_word_from_port(uint16_t address)
{
  return (uint16_t)read_word_from_port(address);
}

EXTERNAL(func_uint32_uint32, read_double_word_from_port)
uint32_t tlib_read_double_word_from_port(uint16_t address)
{
  return read_double_word_from_port(address);
}

EXTERNAL(action_uint32_uint32, write_byte_to_port)
void tlib_write_byte_to_port(uint16_t address, uint8_t value)
{
  return write_byte_to_port(address, value);
}

EXTERNAL(action_uint32_uint32, write_word_to_port)
void tlib_write_word_to_port(uint16_t address, uint16_t value)
{
  return write_word_to_port(address, value);
}

EXTERNAL(action_uint32_uint32, write_double_word_to_port)
void tlib_write_double_word_to_port(uint16_t address, uint32_t value)
{
  return write_double_word_to_port(address, value);
}

EXTERNAL(func_int32, get_pending_interrupt)
int tlib_get_pending_interrupt()
{
  return get_pending_interrupt();
}
