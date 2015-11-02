#ifndef PARSE_H_
#define PARSE_H_

int get_int_argument(int location);
const char * get_string_argument(int location);

char **arguments;
int argument_count;

#endif
