Typical workflow in Emul8
=========================

Understanding a demo script
---------------------------

To understand how to write and maintain a script, it's best go through an example demo - *versatile-console*.

Executing a demo script in Emul8 is typically done with the :term:`start` command:

.. code-block:: bash

   (monitor) start @scripts/demos/standalone/versatile-console

This is a shorthand for using both the :term:`include` and :term:`start` commands one after another.

Simplifying peripheral access
+++++++++++++++++++++++++++++

The first command of the script is:

.. code-block:: bash

   using sysbus

This will allow you to use shortened peripheral names, without having to type ``sysbus`` every time you want to access a peripheral name.

Every peripheral can be accessed in Monitor using its full path, but since most peripherals are connected to a predefined system bus, it may be convenient to assume a default ``sysbus.`` prefix.
You may add any other prefix as well.

Creating a board
++++++++++++++++

.. code-block:: bash

   createPlatform Versatile

This command creates one of the predefined platforms.
As a result, it loads an appropriate platform script, in this case *platforms/boards/versatile*.

It is very simple and consists of only two commands:

.. code-block:: bash

    include @platforms/cpus/versatile
    machine LoadPeripherals @platforms/boards/versatile-externals.json

This creates a CPU and loads some additional peripherals, specific to the Versatile board, which are not important at the moment.

The CPU script, located in *platforms/cpus/versatile*, is responsible for loading base peripherals and creating tags.
For example, the following line will create a log entry telling that and unimplemented peripheral ``watchdog`` was accessed, every time there is a read or write on sysbus address range <0x101E1000, 0x101E1FFF>.

.. code-block:: bash

   sysbus Tag <0x101E1000,0x101E1FFF> "watchdog"

Please note that you can use the ``LoadPeripherals`` command at any point while creating a machine.
It is suggested to split JSON files so that each reflects a specific level of abstraction, e.g. CPU level, board level, specific setup level.

CPU setup
+++++++++

At this moment, after ``createPlatform``, the board is ready to be configured for a specific use case, so we are back in the ``versatile-console`` script.

In this scenario we want to boot Linux directly from an ELF file, without any additional bootloader.
To allow this, some preparations have to be done.
The Linux kernel expects a bootloader to provide ID of the board and a memory address where boot arguments are to be found.
This information has to be provided via CPU registers.
We will do this with the following commands:

.. code-block:: bash

   sysbus.cpu SetRegisterUnsafe 0 0x0
   sysbus.cpu SetRegisterUnsafe 1 0x183     # board id
   sysbus.cpu SetRegisterUnsafe 2 0x100     # atags

Please note that thanks to the ``using`` command used at the beginning we can (but don't have to) omit the ``sysbus.`` prefix.

To understand the syntax of the ``SetRegisterUnsafe`` method you can take a look at the available CPU methods.
To achieve this, after creating the platform, type:

.. code-block:: bash

   sysbus.cpu

Here we provide an excerpt from the output:

.. code-block:: bash

   Following methods are available:
    - Void AddBreakpoint (UInt32 addr)
    - String CurrentSymbol (UInt32 offset)
    [...]
    - Void SetRegisterUnsafe (String register, UInt32 value)
    - Void SetRegisterUnsafe (Int32 register, UInt32 value)
    [...]
    - Void WaitForStepDone ()

   Usage:
    sysbus.cpu MethodName param1 param2 ...


   Following properties are available:
    - Int32 CountThreshold
        available for 'get' and 'set'
    - String ElapsedVirtualTimeForMonitor
        available for 'get'
    [...]

   Usage:
    - get: sysbus.cpu PropertyName
    - set: sysbus.cpu PropertyName Value


You can see all of the methods present in the model of the current CPU (in this case - ARMCPU) that are available from the Monitor.

Connectivity
++++++++++++

To enable user interaction with the emulated board we will now create a few connections with the "outside world".
Firstly we will create a console window attached to UART0 - this will be the board's terminal window.

.. code-block:: bash

    showAnalyzer uart0

The ``showAnalyzer`` command takes an existing peripheral as a parameter (note the use of a shortened notation, without the ``sysbus.`` prefix), creates a new terminal window and connects them together.

Creating an external network interface and connecting it to the host is done as follows:

.. code-block:: bash

    emulation CreateSwitch "switch"
    emulation CreateTap "tap0" "tap"
    connector Connect tap switch
    connector Connect smc91x switch

Firstly, two external interfaces are created: a network switch (named "switch") and TAP network interface (named "tap"), connected to the ``tap0`` interface of the host machine.

If such an interface is not available, a prompt window will pop-up, requesting the user to provide a password (provided the user is a valid sudoer).
Please note that after the creation of these interfaces they are available as emulation objects, so they are accessed without double quotes in subsequent commands.

After the necessary interfaces are created, the two subsequent commands are used to connect them together: both the newly created ``tap`` and Versatile's ``smc91x`` network card are connected to the ``switch``, creating a fully usable network setup, accessible from the host machine via the ``tap0`` interface.

Binaries
++++++++

The last part of the script load the binaries which will be executed in the emulation environment.

Binaries can be loaded from the user's local file system or can be downloaded via the HTTP protocol.
The Versatile demo requires two files - the Linux kernel and RootFS on flash memory, both downloaded from the Internet:

.. code-block:: bash

    sysbus LoadELF @http://emul8.org/emul8_files/binaries/versatile--vmlinux-versatile-buildroot--b2f53187e2d5fd0f74e1b0c8922378605052915e false

    machine CFIFlashFromFile @http://emul8.org/emul8_files/binaries/flash_versatile.img-s_8388608-a6f8e77e2f49daa86b77c3365f30299c3180690b 0x34000000 "flash"

The last parameter of the ``LoadELF`` command determines if the file segments should be loaded using their virtual addresses or not, as in this case, where the physical addresses are used.
This setting depends on the ELF file.

The next command downloads a flash file, creates a flash device named ``flash`` and maps it in memory at 0x34000000.

After the binaries are loaded we provide ATAG information to the kernel:

.. code-block:: bash

    sysbus LoadAtags "console=ttyAMA0,115200 noinitrd root=/dev/mtdblock0 rw rootfstype=jffs2 mtdparts=armflash.0:64m@0x0 earlyprintk mem=256M" 0x10000000 0x100

ATAGs can be provided in plain text format.
They contain information about the console device, rootfs device and format, etc.
Along with the ATAGs the memory size (0x10000000) and the address in memory where this information should be written are given.

Note that it corresponds with the value written to a specific register earlier in this script.

If the provided ELF file does not provide a valid entry point information, you can set it manually:

.. code-block:: bash

    cpu PC 0x8000

Creating a custom emulation
---------------------------

Typically, to prepare a custom emulation you will need both some JSON platform description files, emulation scripts and some binaries.

A lot of platform descriptions and ready-made scripts are already shipped with the framework and are structured as follows:

.. code-block:: bash

    emul8/
    |
    |--> scripts/
    |    |
    |    |--> demos/
    |         |
    |         | standalone/
    |
    |--> platforms/
         |
         |--> boards/
         |
         |--> cpus/

The *platforms* contain , while *scripts/demos/standalone* are example scripts that instantiate the platforms, and put some sample binaries on top of them.

You will want to write at least your own scripts like the demo ones, so that you can setup your boards, binaries and emulation environment according to the needs of your project.

If you are using platforms other than the ones available out of the box, you will also write new emulation scripts and JSON files similar to the ones in the *platforms* directory.

Our proposal is to split your scripts into three separate layers: CPU level, containing description and setup of base CPU peripherals; board level with board-specific devices and execution level responsible for loading binaries and final configuration.

You will then find that quite often you will be able to reuse at least some of the scripts.

If you plan to run multiple machines, you can create a top-level script that will load each machine and create connections between them.
This way you would be able to reuse parts of your solution in further projects.

If the new emulation uses any of the provided boards or CPUs you can either copy the appropriate files to your project directory and load them from there or use them directly in your script.
For example, to use a Versatile board, at the beginning of your script type:

.. code-block:: bash

    mach create
    include @platforms\boards\versatile

This can be followed by loading of binaries, setting up the network, etc.

Please note that all of the paths used in the scripts can be either absolute or relative to the Emul8 root directory.

If you want to use paths relative to the directory where Emul8 is ran, use the $ORIGIN variable instead.

Additionally, HTTP URLs can be used to download files over the network - in that case the files will be locally cached.
