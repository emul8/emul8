#ifndef MAIN_H_
#define MAIN_H_

#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include "consts.h"
#include "parse.h"
#include "actions.h"
#include "device.h"

static const char * HELP_MESSAGE =\
	"Emulator controller for Linux, revision " S(VERSION) "\n"
	"(c) 2011, 2014 ant micro <antmicro.com>\n"
	"Usage:\n"
	PROGRAM_NAME" ACTION [PARAMETERS]\n"
	"where ACTION is one of:\n"
	;

int selected_action;

static void help();
static int parse_action(const char * action);

extern char **arguments;
extern int argument_count;
extern int ACTIONS_NO;
extern action_t ACTIONS[];

#endif
