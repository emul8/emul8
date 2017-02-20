Running and usage
=================

The Emul8 framework provides an API which allows the user to create, assemble and run software models of the hardware to be emulated.

The models are mostly written in C# (although other *mono* runtime languages can also be used).

Using the framework - including assembling new platforms - does not, however, require the knowledge of programming languages, as assembling and manipulating platforms and emulating them can be done through emulation scripts and configuration files, as described in the :doc:`emulation_files` section.

.. note::

   For experienced users, the possibility to write models in C# is a good means to implement things beyond the scope of emulation scripts (which can do what the API provides).
   However, the scripts are the preferred way to achieve most goals.

.. _running-emul8:

Running Emul8
-------------

To run Emul8, use the :program:`run.sh` script, with the optional ``file`` parameter and following options:

.. program:: run.sh

.. option:: -p

   Run in plain mode (do not output colours).

.. option:: -d

   Run in Debug mode (more debug information, but slower).
   Remember to use :option:`build.sh -d` first.

.. option:: -e command

   Execute the given command on startup.
   This option cannot be used together with the ``file`` parameter.

.. option:: -P port

   Instead of opening a Monitor window, listen on a specified port for commands.

.. option:: -h

   Display usage & help information.

``file`` is an optional argument - path to a script that should be loaded on startup.

Monitor
-------

The monitor (CLI) exposes the Emul8 API in the form of objects and methods which can be used like a scripting language.
There are also some special helper commands.

After running the CLI (see :ref:`running-emul8`) you will see the git branch and version denotation of the build you are using, copyright and prompt:

.. code-block:: sh

   Emul8 Framework, version X.X.X.X (branchname-xxxxxxxxxxxxxxxxxxxxx)

   (monitor)

The ``(monitor)`` part denotes the :ref:`context` you are in.

The :kbd:`<TAB>` key provides auto-completion - you can use it whenever you are unsure what to type.
Hitting :kbd:`<TAB>` on an empty terminal will just list all available commands and objects at that moment.

If you want to manipulate a machine/peripheral or object, write ``<object_name>`` and press :kbd:`<TAB>` to see all available options.

Typing any command with incorrect parameters will print a help string, so do not be afraid to experiment.

Issuing :term:`help` will print all available helper commands with a short description.
You can get further help for any command using :term:`help \<command_name\> <help>`.

Some more commonly used commands also have short, usually single-letter, aliases.

Apart from typing commands interactively, you can also execute them from a script - see the :term:`include` command and the :doc:`emulation_files` section.

.. _context:

Context
+++++++

The monitor is contextual with regard to machines, i.e. if you want to manipulate any peripheral which is part of a given machine, you have to enter its context, which is always shown in the prompt.
The initial context after turning on the CLI is always ``(monitor)``.

Changing the context can be done using the :term:`mach set \<machine_name\> <mach>` command first.

:term:`mach add \<machine-name\> <mach>` lets you create a new machine, but note that its context is not entered automatically.

:term:`mach create <mach>` is a shortcut which lets you create a machine with an automatically assigned name and enter its context automatically.

You can use :term:`mach` to list all available machines.

Once in the context of a given machine, you can access its peripherals.
All peripherals are stored in a tree-like structure, with the root being a system bus, available in the machine's context under the name ``sysbus``.
To access them from the Monitor, you can provide their whole path, e.g. ``sysbus.gpioPortA.led``.
The Monitor simplifies access to peripherals with tab completion.
You may try it by typing ``sysbus.`` and pressing :kbd:`<TAB>`.

Since it is tiresome to type fully qualified peripheral names every time, a :term:`using` helper command has been implemented - you will probably find yourself typing :term:`using sysbus <using>` quite often.

Command descriptions
++++++++++++++++++++

.. include:: command_descriptions.rst
