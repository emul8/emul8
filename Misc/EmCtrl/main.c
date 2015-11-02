#include "main.h"

int
main(int argc, char *argv[]) {
  arguments = argv;
  argument_count = argc;
  if(argc < 2) {
    help();
    return 0;
  }
  if(!parse_action(argv[1])) {
    return 2;
  }
  if(connect()) {
    return 1;
  }
  ACTIONS[selected_action].handler();

  return 0;
}

static void
help() {
  printf("%s", HELP_MESSAGE);
  int i;
  for(i = 0; i < ACTIONS_NO; i++) {
    printf("\t%s - %s\n", ACTIONS[i].name, ACTIONS[i].description);
  }
  printf("\n");
} 

static int
parse_action(const char * action) {
  int i;
  for(i = 0; i < ACTIONS_NO; i++) {
    if(strcmp(ACTIONS[i].name, action) == 0) {
      selected_action = i;
      return 1;
    }
  }
  fprintf(stderr, "Error: There is no action named '%s'.\n", action);
  return 0;
}


