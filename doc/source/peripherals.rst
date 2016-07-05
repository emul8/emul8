Writing peripheral models
=========================

Peripherals can be divided into two main groups: those that are connected directly to the system bus and those that are accessible via a dedicated controller, i.e. I2C, SPI, USB and other controllers.
The Emul8 framework allows you to connect them in a very flexible fashion, thanks to specific interfaces implemented by device models.

For bus-accessible peripherals there are three main interfaces: ``IDoubleWordPeripheral``, ``IWordPeripheral`` and ``IBytePeripheral``.
These interfaces provide methods to access a peripheral with, respectively, 32-bit, 16-bit and 8-bit width of registers.
It is possible to implement more than one of such interfaces, or even allow Emul8 to automatically translate between different widths.

Models of peripherals attached to dedicated controllers implement different interfaces.
Feel free to browse the code for implementors of ``IPeripheral`` interface, which is the base interface for all peripherals.

Some models provide either very complicated or very repetitive functions.
Emul8 tries to make implementing such peripherals easier by providing base classes of different peripheral types which can be inherited from.

The most notable case of a complicated peripheral is a timer.
If you want to implement your own model, you are strongly encouraged to either base it on, or contain as a field, ``LimitTimer`` or ``ComparingTimer`` (differing in operation semantics).

Another base type worth mentioning is ``UARTBase``, implementing some base features of a UART, integrating it with the whole environment, like terminal emulator etc.

Registers
---------

Operations on peripherals are usually performed via registers.
The most common case, for peripherals registered on a ``SystemBus``, is accessing a certain offset within a peripheral with write or read operation and with certain access width.
Each offset points to a register and each register may contain many fields.
Fields may have different semantics (e.g. readable, writable, read-to-clear etc.), width and interpretation.
As interpretation of each field within an appropriate read/write method may be cumbersome, a framework for declarative register description was introduced.

Defining registers
++++++++++++++++++

Registers should be kept as private fields of a class, of one of the following types: ``DoubleWordRegister``, ``WordRegister`` and ``ByteRegister``, depending on their width.

.. note::

  Registers of a certain width may be accessed with different access width, but they will be subject to appropriate casting.

The primary interface of registers contains ``Read`` and ``Write`` methods.
These methods analyze the register's fields, execute required callbacks (described later on) and modify the value of the register.
There is also a ``Reset`` method which sets the register to its initial value.
Keep in mind that the ``Read`` method may also modify the value.
To obtain it without any modifications use the ``Value`` property.

Creating registers should only be done once per object lifetime, during its construction.

Defining fields
+++++++++++++++

Fields are building blocks of registers.
There are three types:

* ``FlagRegisterField`` - a 1-bit boolean field,
* ``EnumRegisterField<T>`` - generic field that may represent an enumeration value of a provided type,
* ``ValueRegisterField`` - the most general type of field, with only a numeric representation.

Each field is defined by several attributes, such as position and width, described in the source code documentation.
Here callbacks and field access modes will be described in more detail.

Fields may be accessed with their ``Value`` property.
It does not invoke any callbacks and is not subject to field access mode verification.

It is not necessary, but possible, to keep register fields as class fields.
It is only needed when you need to access the ``Value`` property explicitly.
Most cases can be handled using callbacks.

.. note::

    ``EnumRegisterField<T>`` does not perform any verification regarding the underlying enumeration completeness.
    It is possible to pass a value that is not reflected in the enumeration type.
    However, if the value exceeds the field's width, an ``ArgumentException`` is thrown.

All register field types can be defined using fluent functions on the register objects.
These are namely ``WithFlag``, ``WithValueField``, ``WithEnumField`` and ``WithTag``.

Also, to receive a read-write register with only one value field covering its whole length, a ``CreateRWRegister`` method is available.
It is suitable only for the simplest registers that differ only in their reset value.

Field access mode
+++++++++++++++++

As fields may be declared with different semantics, a deep understanding of available options is necessary.

Access modes are defined in the ``FieldMode`` enumeration.
All modes are verified only during ``PeripheralRegister.Write`` and ``PeripheralRegister.Read`` accesses.

As ``FieldMode`` is defined with ``Flags`` attribute, multiple modes can be defined for one field.
However, there is a limitation that only one "read" flag (:term:`Read` or :term:`ReadToClear`) and one "write" option (all of the remaining) may be set for one field.

.. glossary::

    Read
        Allows the field to be read.
        Without this mode the field is always read as 0.

    ReadToClear
        Allows the field to be read, but with a side effect.
        After every read the field is set to *0* (so the value returned is not yet cleared).

    Write
        Allows the field to be written.

    Set
        Allows the field to be set by writing *1*.
        Writing *0* has no effect.

    Toggle
        Allows the field to be toggled by writing *1* (so it changes from *1* to *0* and from *0* to *1*).
        Writing *0* has no effect.

    WriteOneToClear
        Allows the field to be cleared by writing *1*.
        Writing *0* has no effect.

    WriteZeroToClear
        Allows the field to be cleared by writing *0*.
        Writing *1* has no effect.


Field callbacks
+++++++++++++++

For each field four callbacks may be defined: ``readCallback``, ``writeCallback``, ``changeCallback`` and ``valueProviderCallback``.
The developer can provide handler functions to every callback required.

Handler signatures are similar: ``void FunctionName(T oldValue, T newValue)``, where ``T`` is dependant on field's type (may be ``uint``, ``bool`` or ``enum``).
Only the ``valueProviderCallback`` differs, as it has the following signature: ``T FunctionName(T currentValue)``.

For ``readCallback`` and ``changeCallback`` the ``oldValue`` parameter presents the field's original value before access and ``newValue`` means the current value after the analysis of field access modes.

For ``writeCallback`` the meaning of ``oldValue`` is the same, but ``newValue`` has to be interpreted as the value that was written to the field.
It may be inconsistent with the field's final value, depending on the defined access modes.

For ``valueProviderCallback`` the ``currentValue`` is the field's original value before access and the return value overwrites it.

.. note::

    Callbacks are called sequentially for each field defined in a register.
    The order in which they are called (apart from ``valueProviderCallback``) is not defined.
    It is important to keep in mind that ``newValue`` for ``readCallback`` and ``changeCallback`` may be influenced by previously executed handlers, if they change the value of other fields.

Register.Read
.............

``valueProviderCallback`` is called for each field when a register is read, regardless of its field access flags.
The value returned overwrites the old register value and is eventually passed as ``oldValue`` to ``readCallback``.

``readCallback`` is called for each field when a register is read, regardless of its field access flags.

``changeCallback`` is called for each field that has ``ReadToClear`` flag and has value not equal to zero before being cleared.
Please keep in mind that it is not called for registers with the ``Read`` flag, even if they have a ``valueProviderCallback`` declared.

Register.Write
..............

``writeCallback`` is called for each field when a register is written, regardless of its field access flags.

``changeCallback`` is called for each field that has any write access flag set and its current value is affected by this write operation.

Register callbacks
++++++++++++++++++

Similarly to field callbacks, the developer can register callbacks for the whole registers.
There are three callback types available: ``readCallback``, ``writeCallback`` and ``changeCallback``.

Their semantics is identical to their field counterparts.
Read callbacks are called on each register read, write - on each register write, and change callbacks are called whenever there is any change to register value, regardless of the type of operation.

Please note that fields' ``valueProviderCallbacks`` to not trigger register's ``changeCallback``.

For each callback type there may be many registered functions.
They are called in an undefined order.

Unhandled fields
++++++++++++++++

It is considered a good practice to implement only these fields that are fully supported and/or required.
However, it is also important to receive an information whenever other fields are written by software.

To log an information about unhandled fields being written to, use ``PeripheralRegister.Tag`` method.

Register collections
++++++++++++++++++++

To simplify read and write methods in peripherals, three collections are available: ``DoubleWordRegisterCollection``, ``WordRegisterCollection`` and ``ByteRegisterCollection``.
They map individual registers with their offsets, offering ``Read``, ``Write`` (both with corresponding ``Try`` versions) and ``Reset`` methods.

This is especially useful and recommended for registers that do not have an extensive logic behind them and do not need to be accessed explicitly as class fields.

It is possible to mix the classic, switch-based approach with the collection-based one, e.g. by accessing the collection on the **default** case of a **switch** block.
