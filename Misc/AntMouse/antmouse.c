#include <linux/module.h>
#include <linux/input.h>
#include <linux/init.h>

#include <asm/irq.h>
#include <linux/irqreturn.h>
#include <linux/interrupt.h>
#include <asm/io.h>
#include "antmouse.h"

static struct input_dev *antmouse_dev;
static int *regs;

static irqreturn_t irq_handler(int irq, void *dummy) {
  int newx, newy;
  newx = regs[XREG];
  newy = regs[YREG];

  input_report_abs(antmouse_dev, ABS_X, newx);
  input_report_abs(antmouse_dev, ABS_Y, newy);
  input_report_key(antmouse_dev, BTN_TOUCH, regs[LEFTBTN]);
  input_report_key(antmouse_dev, BTN_LEFT, regs[LEFTBTN]);
  

  input_sync(antmouse_dev);
  regs[INTERRUPT] = 1;
	return IRQ_HANDLED;
}

int init_module(void) {
	int retval;

  antmouse_dev = input_allocate_device();
  regs = ioremap(ANTMOUSE_BASE, ANTMOUSE_SIZE); 

  set_bit(EV_KEY, antmouse_dev->evbit);
  set_bit(EV_ABS, antmouse_dev->evbit);
  input_set_abs_params(antmouse_dev, ABS_X, 0, ANTMOUSE_WIDTH, 1, 1);
  input_set_abs_params(antmouse_dev, ABS_Y, 0, ANTMOUSE_HEIGHT, 1, 1);

  set_bit(BTN_LEFT, antmouse_dev->keybit);
  set_bit(BTN_RIGHT, antmouse_dev->keybit);
  set_bit(BTN_TOUCH, antmouse_dev->keybit);

  set_bit(ABS_X, antmouse_dev->absbit);
  set_bit(ABS_Y, antmouse_dev->absbit);

  antmouse_dev->name = "EP0700M06";

  retval = input_register_device(antmouse_dev);

  if(request_irq(ANTMOUSE_IRQ_NUMBER, irq_handler, 0, "antmouse", NULL)) {
    printk(KERN_ERR "antmouse.c: Can't allocate irq.");
    return -EBUSY;
  }

  return retval;
}

void cleanup_module(void) {
  input_unregister_device(antmouse_dev);
  input_free_device(antmouse_dev);
  free_irq(ANTMOUSE_IRQ_NUMBER, irq_handler);
  iounmap(regs);
}

MODULE_LICENSE("GPL");
MODULE_AUTHOR("Ant Micro <www.antmicro.com>");
MODULE_DESCRIPTION("antmouse");
