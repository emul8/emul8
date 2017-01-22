Debugging
=========

Execution Mode
--------------

Normally, a CPU executes code in so-called *continuous* mode, i.e., until a pause command is issued by the user or any breakpoint/watchpoint is hit.

It is possible, however, to switch into *single step* mode where the CPU is halted after every single instruction. In this scenario stepping is necessary to advance::

    sysbus.cpu ExecutionMode SingleStep
    sysbus.cpu Step [number of instructions]
    sysbus.cpu ExecutionMode Continuous

where:

* ``sysbus.cpu`` - the name of the CPU peripheral;
* ``number of instructions`` - optional number of instructions to execute (default 1).

Breakpoint hook
---------------

A breakpoint hook is triggered whenever the CPU executes an instruction located at a given address. To add a breakpoint hook use::

    sysbus.cpu AddHook (address) (script)

where:

* ``sysbus.cpu`` - the name of the CPU peripheral that should break on execution;
* ``address`` - address on which the hook is set;
* ``script`` - python script that is executed whenever the hook is triggered.

Within the script, two local variables are available:

* ``pc`` - the address on which the hook was executed;
* ``self`` - the CPU object the hook belongs to.

Multiple hooks can be used with the same address. There is no guarantee on the order of execution for multiple hooks.

All hooks created at a given address (including ones created by GDB) can be removed using::

    sysbus.cpu RemoveHooksAt (address)

.. note::

  It is not possible to remove a single hook from the command line interface.

All created hooks (including ones created by GDB) can be removed using::

    sysbus.cpu RemoveAllHooks

Watchpoint hook
---------------

A watchpoint hook is triggered whenever a given address on a bus is accessed.
The user can specify the parameters according to which a given watchpoint hook is triggered, read or write, and width.

The command to create a watchpoint hook is::

    sysbus AddWatchpointHook (address) (width) (access) (script)

where:

* ``address`` - bus address on which the hook is set;
* ``width`` - width of the access on which the hook should be triggered (1, 2, 4, 8); multiple values can be used (with logical OR, e.g., 6 for double word and word);
* ``access`` - the kind of access the hook is triggered on i.e., read, write or both;
* ``script`` - python script that is executed whenever the hook is triggered.

Within the script three local variables are available:

* ``address`` - the address on which the hook was set;
* ``width`` - the actual width of the access;
* ``self`` - points to the ``SystemBus`` object the hook belongs to.

Multiple hooks can be used with the same address.

.. note::

    A watchpoint hook is triggered whenever a given part of a bus is accessed.
    Please notice that this is possible not only from a CPU.
    It can be accessed by any other peripheral, for instance DMA.
    Also, note that a hook is only triggered when byte/word/double word access is used.
    In other words, it will not be triggered when ReadBytes/WriteBytes methods are used for the access (which again may happen with DMA).

All hooks created at a given address can be removed using::

    sysbus RemoveAllWatchpointHooks (address)

GDB support
-----------

Aside from built-in debugging mechanisms, Emul8 supports the remote GDB protocol.
In order to control CPU execution from GDB, use::

    sysbus.cpu StartGdbServer (port)

where:

* ``sysbus.cpu`` - the name of the CPU peripheral
* ``port`` - port number to listen for GDB

This will put CPU into ``SingleStep`` execution mode which will block before executing every instruction waiting for GDB commands.

To connect GDB to Emul8 and optionaly load symbols, run GDB and execute (**in the GDB console**)::

    target remote :(port)
    file (path to file)

where:

* ``port`` - the same port number as chosen in Emul8
* ``path to file`` - path to binary file with symbols

To stop the GDB server execute::

    sysbus.cpu StopGdbServer
