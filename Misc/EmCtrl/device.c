#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <errno.h>
#include <fcntl.h>
#include <sys/mman.h>
#include <stdint.h>
#include "consts.h"

volatile uint32_t *device_map;

void
write_register(int reg_no, uint32_t value) {
  device_map[reg_no] = value;
}

uint32_t
read_register(int reg_no) {
  return device_map[reg_no];
}

void
write_string(const char *str) {
  int length = strlen(str);
  if(length > STRING_MAX_SIZE) {
    fprintf(stderr, "Error: string to write to device is too long. Maximum length is %d.\n", STRING_MAX_SIZE);
    exit(1);
  }
  char *destination = (char*)device_map;
  destination += STRING_OFFSET;
  strcpy(destination, str);
}

void
read_string(char *str) {
  char *source = (char*)device_map;
  source += STRING_OFFSET;
  strcpy(str, source);
}

int activate() {
  uint32_t magic = device_map[0];
  if(magic != MAGIC+VERSION)
  {
      fprintf(stderr, "Error: failed to activate device, received magic %X when %X was expected.\n", magic, MAGIC+VERSION);
      return 1;
  }
  device_map[0] = magic;
  return 0;
}

int
connect() {
  int mem_device = open("/dev/mem", O_RDWR | O_SYNC);
  if(mem_device < 0) {
    fprintf(stderr, "Error while opening /dev/mem: %s.\n", strerror(errno));
    return 1;
  }
  device_map = mmap(NULL, EMULATOR_CONTROLLER_SIZE, PROT_READ | PROT_WRITE, MAP_SHARED, mem_device, EMULATOR_CONTROLLER_BASE);
  if(device_map == (void *)-1) {
    fprintf(stderr, "Error while doing memory map on device at 0x%X: %s.\n", EMULATOR_CONTROLLER_BASE, strerror(errno));
    return 1;
  }

  /* now handshaking */
  return activate();
}
