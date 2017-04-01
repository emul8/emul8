Time and synchronization
========================

Emulation time unit
-------------------

*Emulation time unit* is a time unit widely used in emulator.
Its value is 0.000001s, i.e. one microsecond.
In other words, timer producing clock event every *emulation time unit* has frequency of 1GHz.

Deterministic execution
-----------------------

Since the flow of the program is unambiguously defined, the only reason for a given execution to run differently from a previous one is that these executions encountered interrupts in different moments with regard to the number of instructions executed so far.

Some interrupts are associated with external peripherals, usually input devices such as a mouse or keyboard.
In that case the differences between executions are natural, because they would also happen on a real hardware.

With timers, however, this is not the case.
Since in emulation timers are tied to the host computer clock and the speed of the execution varies, the execution is not deterministic by default, unlike in real hardware.
To resolve that problem, in Emul8 it is possible to change the source that is driving timers - the *clock source*, and choose an emulated CPU as such.

The clock source drives all the timers in a given machine and can be changed at will.

To set the clock source of a given machine, use ```SetClockSource``:

.. code-block:: bash

    (machine-0) machine SetClockSource name_of_the_clock_source

where ``name_of_the_clock_source`` will usually be a CPU - for deterministic execution.

Throughout this documentation we sometimes also refer to deterministic execution as *using deterministic timers*.

When a CPU is used as a clock source, the timers are driven based on the number of instructions executed so far.
There are two parameters that control how it is done.

The first is a property of the CPU called ``PerformanceInMIPS``.
This is directly used to convert number of instructions executed so far to elapsed (virtual) time.
For example, if the value of this property is 100, then a virtual second passes after 100 million of instructions are executed by the CPU.

Timers are not necessarily updated on each instruction executed.
This is controlled by another property - called ``CountThreshold`` which says after how many instructions timers will be updated.

To go back to host time clock source (which is the default one), use:

.. code-block:: bash

    (machine-0) machine SetHostTimeClockSource


Domains
-------

The key concept in machine synchronization is a *synchronization domain*.
Such a domain is a set of machines and externals - these will be synchronized in one domain.

What does this mean?

With deterministic timers enabled, each machine is able to execute for at most one *sync time unit* [1]_ before having to wait for other machines in the same domain to reach the same point of execution in its virtual time sense.
This means that in any moment of time two machines can only differ by no more than a sync time unit and there are moments when they are exactly in the same point in virtual time.

Such sync points are very important; only during these any communication - like ethernet networking for instance - between machines can happen.
In other words no matter when the message is transmitted by some peripheral to the external world (e.g. another machine), it will be delivered in the nearest synchronization point.
No execution takes place when that message is transmitted; machines are halted until all transmissions are finished.

With such properties the whole synchronization domain can execute deterministically.

Although communication can be initialized (a message issued) at any moment between two synchronization points, it can only be received by a given machine at a specific moment (in the synchronization point).
Such a feature of communication can be preserved because the externals are also in the synchronization domain.
When the given synchronization point is reached, all machines are halted a little bit longer - this is the time for externals to exchange messages.
Note that it is also necessary to assume that communication between machines can only happen through an external.

However, despite the usage of sync points, external events like user input to serial ports will alter execution.
To save the feature of deterministic execution in the presence of such events, you can use :ref:`recording`.

Usage of synchronization domains
--------------------------------

.. note::

    The API presented here is only temporary and may be subject to change.

Synchronization domains are stored in the emulation.
Each domain has its own numerical id.
A domain can be created using:

.. code-block:: bash

   (monitor) emulation AddSyncDomain

The id of a newly created synchronization domain is returned.
Then, such a domain can be attached to the machine (or rather, machine put in a synchronization domain) using:

.. code-block:: bash

    (machine-0) machine SetSyncDomainFromEmulation 0

where ``0`` is the domain id.

Usage with externals is identical.
That is, if ``switch`` is our example external, then issuing:

.. code-block:: bash

    (monitor) switch SetSyncDomainFromEmulation 0

makes it synchronized in domain 0.
Note that you can only set the synchronization domain if you're using deterministic timers, that is, host time clock source is not used.

To find out which machines and externals are in the given synchronization domain, you can execute:

.. code-block:: bash

    (STM32F4_Discovery-kit-4) emulation GetElementsInSyncDomain 0
    -------------------------
    |SyncDomain 0           |
    -------------------------
    |STM32F4_Discovery-kit-0|
    |STM32F4_Discovery-kit-1|
    |STM32F4_Discovery-kit-2|
    |STM32F4_Discovery-kit-3|
    |STM32F4_Discovery-kit-4|
    |switch                 |
    -------------------------

where 0 is naturally the number of the domain.

Tweaking and customization
--------------------------

There are two parameters related to synchronization domains, both are called *sync units*, but one belongs to the given synchronization domain and the second one is the property of a given machine.
Let's start with the latter: it tells how many instructions can be executed (expressed in the basic time units) by the given machine until the synchronization domain is notified.

The sync unit of the time domain, respectively, tells how many such events should happen before the actual synchronization takes place.
You can change the sync unit of respective machines to change relative virtual speeds of the machines and use the sync unit of the domain to adjust the frequency of synchronization.

Note that the value of the sync unit is a tradeoff - the higher it is, the more parallel the execution, however the machines are synchronized less frequently.
Execution is still deterministic in that case, but the user can experience local desynchronisation between sync points.
Also all messages between machines are exchanged less frequently which can influence execution.
For example, network transmission may experience higher transmission times.

Hooks
-----

.. note::

    The API presented here is only temporary and may be subject to change.

The ``SynchronizationDomain`` class provides a hook mechanism.
A hook is executed after all deferred actions from externals and before the machines resume execution.

The number of synchronization points so far (*synchronization count*) is provided to the hook.
You can use Python scripts provided as a string to be executed at a given hook.

The ``self`` variable is tied to the emulation from which the sync domain was used when the hook was created.
The ``syncCount`` variable contains the synchronization count.

A hook can be added using a command like the one below:

.. code-block:: bash

    (monitor) emulation SetHookAtSyncPoint 0 "self.DebugLog('Synced (%d times)' % syncCount)"

In the example above the emulation will issue a log message with the synchronization count on each sync point.
``0`` stands for the synchronization domain id.
To remove a hook you can use:

.. code-block:: bash

    (monitor) emulation ClearHookAtSyncPoint 0

Again, ``0`` is the synchronization domain id.

.. _recording:

Recording
---------

Even with deterministic (i.e. CPU-based) timers and synchronization domains, external events will still influence execution.
To provide deterministic behaviour in such a case, you have to not only save such events during one run and replay them during another, but also have to be sure that they happen at the exactly same moments (with respect to virtual time) that they happened in the first place.

This is what the recording infrastructure is for.

Usage
+++++

For any machine you can set up a file to which events will be recorded.
Such a file can later be used to replay events for a given machine - for the file to work the machine has to contain the same peripherals, named in an identical way (this only applies to peripherals on which external events happened).

Recording can be done in two *modes*.

In the first mode called *DomainExternal*, only events *external* to the domain the recorded machine is in are saved.
For example, user input via UART or mouse will be recorded, but the network packet transmitted via switch which is in the same synchronization domain - won't.
Thanks to that, you can record events for each machine in the synchronization domain and then replay all machines - communication within such a domain will also be repeated due to determinism so recording only external events is enough.

On the other hand, sometimes you may want to record only a single machine located in the broader synchronization domain and then replay all events, even those coming from within the domain, since other machines will not be replayed at all.

This is what the second mode - *All* - is for (all events are recorded in this mode).
This way you can record in a multimachine environment and then replay (with all communication etc.) only one machine - only one machine is then to be emulated which gives the user better performance.

To record events on a given machine issue:

.. code-block:: bash

    (machine-0) machine RecordTo @file.dat DomainExternal

or

.. code-block:: bash

    (machine-0) machine RecordTo @file.dat All

``file.dat`` is naturally the name of the file containing the recording.
The first command records only events external to the domain the machine is in, the second one - all of them.

The file can be later used to replay events.
To do that use:

.. code-block:: bash

    (machine-0) machine PlayFrom @file.dat

Fast replay
-----------

Normally deterministic execution happens as fast as possible with the important exception of the WFI-class instructions.
If the CPU goes to sleep, the time to the nearest interrupt is calculated and then thread sleeps for such time.
Since CPUs sleep a lot in the typical scenario, this gives the user an experience similar to a native execution, ensuring that virtual time more or less follows the host time.
During replay, however, this can be unnecessary, since typically the user would like to go to the given point in virtual time as fast as possible (for example to investigate a bug that happens at the given moment of time).

This is where fast replay comes handy.
You only need to set the ``AdvanceImmediately`` property of the CPU to ``true``:

.. code-block:: bash

    (machine-0) cpu AdvanceImmediately true

In this mode WFI-like instruction are finished immediately and the virtual time is advanced with the value to the nearest interrupt.
Disable this mode by setting the property to ``false`` instead.


.. [1] A sync time unit is a synchronization parameter. Its value is given in *emulation time units*.

Real time clock
===============

Since, to be deterministic, the execution must always encounter the same input data to behave the same way, implementing a real time clock by tying it to the clock of the host can effectively break the determinism.
Therefore three possible modes of a real time clock are available to user:

* ``VirtualTime`` - the base time used for the real time clock is 1970-01-01 plus elapsed virtual time;
* ``VirtualTimeWithHostBeginning`` - as above but instead of 1970-01-01, the current date for the moment when the machine was started is used.

This option can be changed by setting the ``RealTimeClockMode`` property of the machine.
