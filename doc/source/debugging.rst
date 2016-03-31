Debugging
=========

Watchpoint hook
---------------

A watchpoint hook is triggered whenever a given address on a bus is accessed.
The user can specify the parameters according to which a given watchpoint hook is triggered, read or write, and width.

The command to create a watchpoint hook is:

.. code-block:: bash

    sysbus AddHookOnWatchpoint (address) (width) (access) (script)

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
