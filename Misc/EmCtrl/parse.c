#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include "actions.h"
#include "parse.h"

static void check_no_of_arguments(int location);
extern int selected_action;

int get_int_argument(int location) {
  check_no_of_arguments(location);
  return atoi(arguments[location]);
}

const char * get_string_argument(int location) {
  check_no_of_arguments(location);
  return arguments[location];
}

static void check_no_of_arguments(int location) {
  if(argument_count <= location) {
    fprintf(stderr, "Error: action %s requires more arguments.\n", get_action_name(selected_action));
    exit(1);
  }
}
