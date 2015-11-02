obj-m += antmouse.o

all:
	make ARCH=arm CROSS_COMPILE=arm-none-linux-gnueabi- -C /home/konrad/antmicro/enclustra-linux-TFT M=$(PWD) modules

clean:
	make ARCH=arm CROSS_COMPILE=arm-none-linux-gnueabi- -C /home/konrad/antmicro/enclustra-linux-TFT M=$(PWD) clean
