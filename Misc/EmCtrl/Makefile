CROSS_COMPILER ?= arm-none-linux-gnueabi-gcc
CC = $(CROSS_COMPILER)
OBJS = actions.o main.o parse.o device.o
CFLAGS += -Wall --pedantic -O3 -std=c99

all: emctrl

emctrl: $(OBJS)
	@$(CC) $(LFLAGS) $(OBJS) -o emctrl
	@echo " LD $<"

%.o: %.c
	@$(CC) $(CFLAGS) -c -o $@ $<
	@echo " CC $<"

clean:
	@rm -f *.o
	@echo " CLEAN"
