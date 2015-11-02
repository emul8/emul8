#ifndef DEVICE_H_
#define DEVICE_H_

#include <stdint.h>

extern volatile uint32_t *device_map;

int activate();
void write_register(int reg_no, uint32_t value);
uint32_t read_register(int reg_no);
void read_string(char *str);
void write_string(const char *str);
int connect();

#endif
