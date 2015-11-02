#ifndef CONSTS_H_
#define CONSTS_H_

#define PROGRAM_NAME "emctrl"

#define SAVE 1
#define LOAD 2
#define MAKE_PTY 3
#define QUIT 4
#define RECEIVE 5
#define SEND 6
#define SEND_RECEIVE_CTRL 7
#define GET_SET 8
#define LIST 9 
#define DATE 10
#define STOPWATCH 11

#define STOPWATCH_START 0
#define STOPWATCH_STOP 1
#define STOPWATCH_RESET 2

#define EMULATOR_CONTROLLER_BASE 0x20000000
#define STRING_OFFSET 0x100
#define STRING_MAX_SIZE 0x100
#define FILE_OFFSET 0x200
#define FILE_PACKET_MAX_SIZE 0x10000
#define EMULATOR_CONTROLLER_SIZE (FILE_OFFSET + FILE_PACKET_MAX_SIZE)

#define MAGIC 0xDEADBEEF
#define VERSION 3
#define H(s) # s
#define S(s) H(s)
#endif
