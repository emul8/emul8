#include <unistd.h>
#include <stdint.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include "actions.h"
#include "parse.h"
#include "device.h"
#include "consts.h"


static void do_save();
static void do_load();
static void do_receive();
static void do_send();
static void do_get();
static void do_set();
static void do_list();
static void do_date();
static void do_start();
static void do_stop();
static void do_reset();

action_t ACTIONS[] =
{
  { .name = "save", .handler = do_save, .description = "CHECKPOINT_NUMBER - saves checkpoint" },
  { .name = "load", .handler = do_load, .description = "CHECKPOINT_NUMBER - loads checkpoint" },
  { .name = "receive", .handler = do_receive, .description = "HOST_FILE_NAME LOCAL_FILE_NAME - receives file from host" },
  { .name = "send", .handler = do_send, .description = "LOCAL_FILE_NAME HOST_FILE_NAME - sends file to host" },
  { .name = "get", .handler = do_get, .description = "KEY - gets value associated to the given key" },
  { .name = "set", .handler = do_set, .description = "KEY VALUE - sets value associated to the given key" },
  { .name = "list", .handler = do_list, .description = "list the possible keys available for 'get' and 'set'" },
  { .name = "date", .handler = do_date, .description = "prints date in the UNIX format" },
  { .name = "start", .handler = do_start, .description = "starts time measurement" },
  { .name = "stop", .handler = do_stop, .description = "stops time measurement" },
  { .name = "reset", .handler = do_reset, .description = "resets time measurement" }
};

int ACTIONS_NO = sizeof(ACTIONS)/sizeof(action_t);

static void
do_save() {
  uint32_t checkpoint_number = (uint32_t)get_int_argument(2);
  write_register(SAVE, checkpoint_number);
}

static void
do_load() {
  uint32_t checkpoint_number = (uint32_t)get_int_argument(2);
  write_register(LOAD, checkpoint_number);
}

static void
do_receive() {
  const char * local_file_name = get_string_argument(3);
  const char * host_file_name = get_string_argument(2);
  int fd = open(local_file_name, O_CREAT | O_TRUNC | O_WRONLY, S_IWUSR | S_IRUSR); // TODO: permissions read from host
  if(fd < 0) {
    fprintf(stderr, "Error on opening local file %s: %s.\n", local_file_name, strerror(errno));
    exit(1);
  }
  volatile char * buffer = ((char *)device_map) + FILE_OFFSET;
  write_string(host_file_name);
  uint32_t perms = read_register(RECEIVE);
  if(perms == 0) {
    fprintf(stderr, "Error while opening file %s on host.\n", host_file_name);
    close(fd);
    unlink(local_file_name);
    exit(1);
  }
  int received;
  while((received = read_register(SEND_RECEIVE_CTRL))) {
    int written = 0;
    while(written < received) {
      int this_turn_written = write(fd, (void *) (buffer + written), received - written);
      if(this_turn_written <= 0) {
        fprintf(stderr, "Error while writing: %s.", strerror(errno));
	close(fd);
	unlink(local_file_name);
        exit(1);
      }
      written += this_turn_written;
    }
    
  }
  if(chmod(local_file_name, perms)) {
    fprintf(stderr, "Error writing file perrmisions: %s.", strerror(errno));
    close(fd);
    exit(1);
  }
  close(fd);
}

static void
do_send() {
  const char * local_file_name = get_string_argument(2);
  const char * host_file_name = get_string_argument(3);
  int fd = open(local_file_name, O_RDONLY);
  if(fd < 0) {
    fprintf(stderr, "Error on opening local file %s: %s.\n", local_file_name, strerror(errno));
    exit(1);
  }
  volatile char * buffer = ((char *)device_map) + FILE_OFFSET;
  write_string(host_file_name);
  if(read_register(SEND) != 0) {
    fprintf(stderr, "Error while opening file %s on host.\n", host_file_name);
    close(fd);
    exit(1);
  }
  int bytes_read;
  while((bytes_read = read(fd, (void *)buffer, FILE_PACKET_MAX_SIZE))) {
    if(bytes_read == -1)
    {
      fprintf(stderr, "Error while reading file: %s.\n", strerror(errno));
      exit(1);
    }
    write_register(SEND_RECEIVE_CTRL, bytes_read);
  }
  write_register(SEND_RECEIVE_CTRL, 0); // end of transmission
  struct stat stat_str;
  stat(local_file_name, &stat_str);
  write_register(SEND_RECEIVE_CTRL, stat_str.st_mode & 0777);
  close(fd);
}

static void
do_get() {
  const char * key = get_string_argument(2);
  write_string(key);
  uint32_t result = read_register(GET_SET);
  if(!result) {
    fprintf(stderr, "Value for the key %s is not set.\n", key);
    exit(1);
    return;
  }
  char * string = malloc(STRING_MAX_SIZE);
  read_string(string);
  printf("%s", string);
  free(string);
}

static void
do_set() {
  const char * key = get_string_argument(2);
  const char * value = get_string_argument(3);
  write_string(key);
  write_register(GET_SET, 0);
  write_string(value);
  write_register(GET_SET, 0);
}

static void
do_list() {
  char * string = malloc(STRING_MAX_SIZE);
  while(read_register(LIST)) {
  	read_string(string);
	printf("%s\n",string);
  } 
  free(string);
}

static void 
do_date() {
  write_register(DATE, 0);
  char * string = malloc(STRING_MAX_SIZE);
  read_string(string);
  printf("%s", string);
  free(string);
}

static void
do_start() {
  write_register(STOPWATCH, STOPWATCH_START);
}

static void
do_stop() {
  write_register(STOPWATCH, STOPWATCH_STOP);
}

static void
do_reset()
{
  write_register(STOPWATCH, STOPWATCH_RESET);
}

const char *
get_action_name(int action) {
  if(action < ACTIONS_NO) {
    return ACTIONS[action].name;
  }
  return NULL;
}

