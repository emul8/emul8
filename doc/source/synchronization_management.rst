Synchronization
===============

Concept
-------

The basic concept behind synchronization is quite simple.
Machines can execute in parallel, but - from time to time - they should wait for others to reach the same point in virtual time and then continue parallel execution.
When that point is reached, externals that are used to exchange messages between machines (like a network switch) can deliver queued messages.
Thanks to this mechanism you can have deterministic execution with multiple machine emulation (i.e. determinism can be applied to the whole environment).
All machines and externals that are synchronized are said to be in *one synchronization domain*.

Interfaces and implementation
-----------------------------

``ISynchronized`` is the most basic interface.
It represents an element of synchronization domain.

The ``Machine`` class implements this interface and so should all externals that are to be synchronizable.
Usually this applies to all externals that send messages affecting other emulation elements.
If the external is of slave nature (for example it only is used as some kind of detector), then synchronization isn't usually necessary.

The interface contains only one property:

.. code-block:: csharp

    ISynchronizationDomain SyncDomain { get; set; }

Setting the domain is equal to being added to such domain and exiting the previous one.
It is the responsibility of the developer to set the initial domain do ``DummySynchronizationDomain``.
For externals you can also use a base class ``SynchronizedExternalBase`` that already takes care of this initialization.

When the external belongs to a domain (i.e. the ``SyncDomain`` field is set), it must not deliver events to other machines directly.
Instead it should use the domain to order such deliveries.

In other words instead of writing:

.. code-block:: csharp

    SendNetworkPacket();

you should write:

.. code-block:: csharp

    SyncDomain.ExecuteOnNearestSync(SendNetworkPacket);

or, if it extends ``SynchronizedExternalBase``:

.. code-block:: csharp

    ExecuteOnNearestSync(SendNetworkPacket);

External events
---------------

Introduction
++++++++++++

An external events infrastructure is necessary to provide deterministic execution when other than internal sources of interrupts are encountered.
This applies to all user input (like keyboard, mouse) but also to network cards if they are connected to the switch in a different synchronization domain.
Each peripheral that handles external events should follow the pattern set below.

Let's say that an external event is handled by some ``Handler`` method.

.. code-block:: csharp

    public void Handler(T argument)
    {
        // handler code
    }

Normally the body of this method would change the internal state of the peripheral, possibly resulting in transmitting an interrupt to the CPU.
With the new approach such method body should actually be contained in a private method with an identical signature:

.. code-block:: csharp

    private void HandlerInner(T argument)
    {
        // handler code
    }

and the public method code should look as follows:

.. code-block:: csharp

    public void Handler(T argument)
    {
        machine.ReportForeignEvent(argument, HandlerInner);
    }

A version with two event arguments is also available.
That's all what is necessary for a peripheral to be compatible with event synchronization and recording.

Thread sentinel
+++++++++++++++

Since the approach presented above is merely a convention, there exists a way to verify if the convention is followed.
A mechanism called *thread sentinel* can help verify if an interrupt event is delivered to the CPU from an unsynchronized thread.
To enable it, just set the property ``ThreadSentinelEnabled`` of the ``TranslationCPU`` class to ``true``.
Then, whenever a CPU receives such an event, it issues a warning in the following form: ``An interrupt from the unsynchronized thread.``
