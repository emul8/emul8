Native/managed bindings
=======================

It is quite frequent to call the native functions from managed code or vice versa.
A small framework simplifying such operations was developed.
It provides the functionality to bind a class with a native shared object (``.so``) library.
The native-to-managed and vice versa calls will be called "cross domain" in this document.

Common setup
------------

The main class that enables the bindings is named ``NativeBinder``.
Its only public part is the constructor, whose parameters are: the object to bind (i.e. usually ``this``) and path of the library (shared object) file.
There are some important remarks here:

* The constructed object of ``NativeBinder`` must be alive to enable cross domain calls.
  The usual solution is to hold the reference in bound class' field.
* The native binder object must be disposed in order to unload the shared library and free the memory it takes.
* The construction of the native binder must take place before any cross domain calls.

Managed to native calls
-----------------------

It is important to familiarize with the syntax of the delegates used in cross domain bindings.
Because of limitations of the P/Invoke API, the generic ``Func<>`` and ``Action<>`` cannot be used.
Instead, the user of the framework can utilize a set of similarly named pre-generated delegates.

Here are the naming rules of such delegates:

1. For a function that does not return any value, the name starts with ``Action``.
2. For a function returning a value, the name starts with ``Func`` and the type of that value.
3. After that, the type of the arguments used, in the left to right order.
4. If the type has aliased names, the fully qualified one is used.

Few examples to clarify:

* a function that returns ``uint``::

    FuncUInt32

* a function that returns ``int`` and takes one ``uint`` parameter::

    FuncInt32UInt32

* a function that takes a string and return no value::

    ActionString

* a function that takes no argument and returns no value::

    Action

In order to use managed to native calls, it is necessary to make a class field whose type is one of the delegates as mentioned above and decorate such a field with an ``Import`` attribute.

This attribute takes an optional ``Name`` parameter which holds the name of the function in the native module.
If this parameter is not given, the binder uses the name of the field after performing the conversion of naming conventions.
The conversion is from ``PascalCasing`` to ``underscore_separation``, in other words ``SomeFunctionName`` becomes ``some_function_name``.

After the construction of ``NativeBinder`` the field holds a delegate, which invokes the bound function.
If such a function cannot be found in the native module, an exception is thrown.

Please note that **no type checking is performed** at this level, because the object file does not hold type information.
Bugs in types can, but not necessarily will, result in value corruption or application crash.

Native to managed calls
-----------------------

Similarly to the naming conventions for delegates mentioned in the previous section, there is a naming conventions for typedefs.
The typedef name is the name of equivalent delegate after performing a conversion similar to the one mentioned above.
Therefore the names of the typedefs from the previous section are, respectively:

* ``func_uint32``
* ``func_int32_uint32``
* ``action_string``
* ``action``

In order to use the framework it is needed to include the *emul8_imports.h* header file.
Apart from mentioned typedefs it contains two macros:

* ``EXTERNAL_AS`` -- the macro takes three parameters: typedef, name of the imported class method and name of the function in the native module;
* ``EXTERNAL`` -- this one takes only typedef and the name of the function, the imported name is created from the function name with the ``function_name`` to ``FunctionName`` conversion.

Both macros actually create a global variable that holds the necessary function pointer (that points to the imported function) and a function of the name mentioned above and a proper signature that calls such a pointer.
Although the function pointer could be called directly (without an additional function), it would not be possible to link it directly as a function symbol.
This is the reason for such a function to exist.

Methods intented for export to a native library have to be marked with the ``Export`` attribute.
Methods marked with such an attribute which are not exported generate a warning log entry.

Here are some sample definitions and calls:

.. topic:: BoundClass.cs

   .. code-block:: csharp

      [Export]
      void ResetPeripheral() {}

      [Export]
      int Add(int a, int b) { return a + b; }

      [Export]
      uint Increment(uint what) { return what + 1; }

.. topic:: sample_imports.c

   .. code-block:: c

     #include "emul8_imports.h"

     EXTERNAL(action, reset_peripheral)
     EXTERNAL(func_int32_int32_int32, add)
     EXTERNAL_AS(func_uint32_uint32, Increment, incr)

     reset_peripheral();
     int32_t result = add(2, 3);
     uint32_t another_result = incr(1);

It is customary to provide a header file and an implementation for an imported method.
The example of a correct solution is provided here:

.. topic:: imported_function.h

   .. code-block:: c

      #include "emul8_imports.h"

      void send_irq(uint32_t number);

.. topic:: imported_function.c

   .. code-block:: c

      #include "emul8_imports.h"

      EXTERNAL(action_uint32, send_irq)

Contrary to the managed to native calls, types are checked during binding.
An exception is thrown if the delegate's type is not compatible with the typedef.

A complete example
------------------

The ``TranslationCPU`` class with ``translate-arch-endianess.so`` (where ``arch`` is the target architecture and ``endianess`` is either ``le`` or ``be``) form complete example of the described functionality, so it is a good idea to examine them to see a practical implementation.

Customizing the framework
-------------------------

The collection of available delegate types is generated by the ``types_generate.py`` script.

There are two parameters of this script: the maximal number of function parameters and the types of the parameters involved.
These can be edited in the sources of the script and should be self descriptive.

When speaking about types, it is necessary to provide the name of the C type and the corresponding C# one.
Corresponding means that it is one of the `blittable types`_ or it is the convertible non-blittable type.
In other wordsm standard marshalling rules are followed here.

.. _blittable types: http://msdn.microsoft.com/en-us/library/75dwhxf7.aspx

Limitations
-----------

The current version of this framework does not support systems which use shared libraries in other binary form than ELF.
That means, specifically, that Windows and Mac OS X are currently **not** supported.
To provide support for a new platform, one simple functionality is needed: listing the exported symbol names from a given library file.

Other limitations are essentially the same as those in the P/Invoke bindings.
