Debugging
=========

Execution Mode
--------------

Normally CPU executes code in so-called `continuous` mode, i.e., until the pause command is ordered by a user or any breakpoint/watchpoint is hit.

It is possible, however, to switch into `single step` mode where CPU is halted after every single instruction. In this scenario stepping is necessary to advance:

.. code-block:: bash

    sysbus.cpu ExecutionMode SingleStep
    sysbus.cpu Step [number of instructions]
    sysbus.cpu ExecutionMode Continuous

where:

* ``sysbus.cpu`` - the name of CPU peripheral;
* ``number of instructions`` - optional number of instructions to execute (default 1).

Breakpoint hook
---------------

A breakpoint hook is triggered whenever CPU executes an instruction located at a given address.

.. code-block:: bash

    sysbus.cpu AddHook (address) (script)

where:

* ``sysbus.cpu`` - the name of CPU peripheral that should break on execution;
* ``address`` - address on which a hook is set;
* ``script`` - python script that is executed whenever a hook is triggered.

Within the script two local variables are available:

* ``pc`` - the address on which the hook was executed;
* ``self`` - the CPU object the hook belongs to.

Multiple hooks can be used with the same address. There is no guarantee on the order of multiple hooks execution.

All hooks created at a given address (including ones created by gdb) can be removed by using the command below:

.. code-block:: bash

    sysbus.cpu RemoveHooksAt (address)

where ``address`` has the same meaning as above.

.. note::

  It is not possible to remove single hook from command line interface.

All created hooks (including ones created by gdb) can be removed by using the command below:

.. code-block:: bash

    sysbus.cpu RemoveAllHooks

Watchpoint hook
---------------

A watchpoint hook is triggered whenever a given address on a bus is accessed.
The user can specify the parameters according to which a given watchpoint hook is triggered, read or write, and width.

The command to create a watchpoint hook is:

.. code-block:: bash

    sysbus AddWatchpointHook (address) (width) (access) (script)

where:

* ``address`` - bus address on which a hook is set;
* ``width`` - width of the access on which a hook should be triggered (1, 2, 4, 8); multiple values can be used (with logical OR, e.g., 6 for double word and word);
* ``access`` - the kind of access a hook is triggered on i.e., read, write or both;
* ``script`` - python script that is executed whenever a hook is triggered.

Within the script three local variables are available:

* ``address`` - the address on which the hook was set;
* ``width`` - the actual width of the access;
* ``self`` - points to the ``SystemBus`` object the hook belongs to.

Multiple hooks can be used with the same address.

.. note::

    A watchpoint hook is triggered whenever a given part of a bus is accessed.
    Please notice that this is possible not only from CPU.
    It can be accessed by any other peripheral, for instance DMA.
    Also, note that a hook is only triggered when byte/word/double word access is used.
    In other words, it will not be triggered when ReadBytes/WriteBytes methods are used for the access (which again may happen with DMA).

All hooks created at a given address can be removed by using the command below:

.. code-block:: bash

    sysbus RemoveAllWatchpointHooks (address)

where ``address`` has the same meaning as above.

GDB support
-----------

Aside from built-in debugging mechanisms, ``Emul8`` supports remote ``GDB`` protocol.
In order to control the CPU execution from ``GDB`` execute following command in monitor:

.. code-block:: bash

    sysbus.cpu StartGdbServer (port)

where:

* ``sysbus.cpu`` - the name of CPU peripheral
* ``port`` - port number to listen for GDB

This will put CPU into ``SingleStep`` execution mode which will block before executing every instruction waiting for GDB commands.

To connect ``GDB`` to ``Emul8`` and optionaly load symbols, execute:

.. code-block:: bash

    target remote :(port)
    file (path to file)

where:

* ``port`` - the same port number as choosen in ``Emul8``
* ``path to file`` - path to binary file with symbols

To stop ``GDB`` server execute in ``Emul8``:

.. code-block:: bash

    sysbus.cpu StopGdbServer
