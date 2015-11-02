#ifndef ACTIONS_H_
#define ACTIONS_H_

typedef struct {
  void (*handler)();
  const char * name;
  const char * description;
} action_t;

const char * get_action_name(int action);

#endif
