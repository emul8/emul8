Emulation files
===============

Essentially, the emulation can be created from scratch either as a C# project or assembled by hand using the :term:`Monitor`.
These ways use the Emul8 API directly.

A simpler and recommended way is to use helper files, found in the *scripts/* and *platforms/* directories:

* JSON platform configuration files
* batch scripts building the emulation using the same commands as would normally be issued from the :term:`Monitor`

These two formats will be briefly described below.

JSON Platform configuration files
---------------------------------

Platforms are described using the `JSON format <http://www.json.org>`_ in ordinary text files.
Available platform configurations can be found in the *platforms/* directory and are used to build up an emulated platform from appropriate models.
JSON as a format has the advantage of being human-readable and easy to parse.
Some minor deviation from strict JSON are that:

* multiline comments are allowed in the format of ``/* Comment text */``
* a comma is allowed after the last element of a list or array
* hexadecimal numbers are allowed

.. code-block:: c

   {
     "foo1":["bar", "baz",], /* Comment
                                    text */
     "foo2":
     {
       "foo3": 0xabcdef
     },
   }

Reserved keywords have the "_" character as a suffix. Currently existing keywords are:

* ``"_type"`` - Peripheral class name
* ``"_connection"`` - Connections to other peripheral objects
* ``"_irq"`` or ``"_gpio"`` - GPIO outgoing connections
* ``"_irqFrom"`` or ``"_gpioFrom"`` - GPIO incoming connections

A configuration file consists of a list of nodes. The root node is called ``sysbus`` and is always present and does not have to be described in the configuration file.
Under the root node are first level nodes that describe peripheral objects.
A peripheral object node can have subnodes that further describe properties of the peripheral object or its connection to other peripheral objects.

.. code-block:: c

   {
     "Peripheral_object_A": /* Level 1 node */
     {
       "property1":"value", /* Object property */
       "property2":         /* Object property */
       {
       }
     },
     "Peripheral_object_B": /* Level 1 node */
     {
       "property1":"value", /* Object property */
       "_connection":         /* Object property */
       {
       }
     }
   }

Each peripheral object must have a defined name and type.
The name of the object will be the string that is visible when running the command :term:`peripherals`.
Type is directly related to a peripheral's class.

This example is a UART peripheral based on the PL011 peripheral class.

.. code-block:: c

   {
     "uart0":
     {
       "_type":"UART.PL011",
     }
   }

This example is a Memory peripheral based on the Memory peripheral class.

.. code-block:: c

   {
     "Memory":  /* Peripheral name */
     {
       "_type":"Memory",   /* Peripheral type */
     }
   }

Object specific parameters can be defined and will map towards available arguments in the constructor of the peripheral class.

.. code-block:: c

   {
      "Memory":  /* Peripheral name */
     {
       "_type":"Memory",   /* Peripheral type */
       "size": 0x0100000   /* Memory size in bytes */
     }
   }

The reserved keyword ``"_connection"`` describes how a peripheral interconnects with other peripherals. In many cases a peripheral is connected to the root node ``sysbus``.

To connect to another peripheral object (sysbus in this example), use the name of the object inside the ``"_connection"`` node. Depending on the class of the object there are different object properties to follow.

.. code-block:: c

   {
     "Memory":  /* Peripheral name */
     {
       "_type":"Memory",   /* Peripheral type */
       "size": 0x0100000   /* Memory size in bytes */
       "_connection":
       {
          "sysbus":
          [
           {"address":0},
           {"address":0xC0000000}
          ]
       }
     }
   }

.. note::

   A peripheral object A that is connected to another peripheral object B can be declared out of order within the same configuration file. If they are declared in separate files then the file declaring object B must be loaded before the file declaring object A. The sysbus are always present and does not need to be declared beforehand.

  .. code-block:: c

     {
       "Peripheral_object_A": /* Level 1 node */
       {
         "_connection":
         {
           "Peripheral_object_B"
         }
       },
       "Peripheral_object_B": /* Level 1 node */
       {
       }
     }

Emulation scripts
-----------------

Emulation scripts consist of sequences of commands that could be manually typed into the :term:`Monitor`.
Scripts are read by the :term:`Monitor` using the :term:`include` command.
They can be easily reused by including more general scripts in the more specific ones.

.. literalinclude:: scripts/versatile-console
  :language: bash
  :linenos:

.. literalinclude:: scripts/versatile
  :language: bash
  :linenos:

.. note::

  Please note that the provided demos rely on binaries available on the internet.
  To run these scripts for the first time you have to be on-line, but these are subsequently cached and available locally.
