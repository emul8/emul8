Threads and background operations
=================================

Concept
-------

Most peripheral can be thought of as "slave" peripherals, that is not doing anything on their own but rather reacting to CPU events, e.g. a read from the bus they are attached to.

Sometimes, however, their underlying logic demands some non-slave activity.

It can be polling a socket for input or drawing images with a defined framerate.

This chapter deals with the means to achieve this.

Managed threads
+++++++++++++++

A *managed thread* can be used to execute a piece of code in background.
Such a piece of code is executed periodically with a given frequency if the thread is *started* and - naturally - not executed at all if the thread is *stopped*.

Also note that a managed thread is paused and resumed just as the machine it was created by.

We will now discuss some different options for managed threads.

First of all, managed thread can either be *synchronized* or *unsynchronized*.

The first option only makes sense if deterministic timers are used.

A synchronized managed thread will be executed on the clock source thread in the same place where intermachine synchronization takes place.
An unsynchronized thread is executed on a totally independent thread created exactly for that purpose.

The frequency of the execution can be limited so that the thread will be executed no more often that ordered.
The actual frequency depends on general load of the host machine (if the thread is unsynchronized) or the size of the synchronization unit (if the thread is synchronized).

The frequency of the execution (given in Hz) is measured with respect to host time (if the thread is unsynchronized) or virtual time (otherwise).

From the managed thread's user point of view the API (represented by the ``Emul8.Core.IManagedThread`` interface) is very simple:

.. code-block:: csharp

    void Start();
    void Stop();

A managed thread can be obtained from the ``Machine`` class, using the method presented below:

.. code-block:: csharp

    public IManagedThread ObtainManagedThread(Action action, IPeripheral owner,
        string name = null, int frequency = 0, bool synchronized = true)

``Action`` is the delegate to execute as a piece of code, ``owner`` is the peripheral to which the managed thread belongs.
Using the ``name`` parameter one can specify a name related to this thread (note that peripheral name can already be obtained from to the ``owner`` parameter).
``synchronized`` tells whether the thread is synchronized and ``frequency`` specifies its frequency in Hz.

If the frequency is 0, the thread is executed as often as possible.

.. warning::
    Using frequency 0 is not recommended as it puts a great pressure on the host CPU or the CPU emulation thread.

.. note::

   The code that is passed to be executed in the ``action`` parameter should involve a possibly minimal amount of work (for example, if the work to be done can be conceptually seen as a loop, ``action`` should only include one iteration).
   This is because execution of the managed thread action stalls the CPU emulation thread (in the synchronized case) and starting or stopping the machine or thread can only happen between such executions.

Executing an action after some time
+++++++++++++++++++++++++++++++++++

Sometimes, in a peripheral, there is a need to execute an action, but not immediately.

For instance, assume we have a DMA operation to perform.

Many drivers wouldn't be ready for to finish immediately, so it may be reasonable to execute it after some time delay.

However, you should not use normal threads for that due to their influence on execution determinism.
On the other hand, managed threads are too cumbersome for such a scenario.
There is a specialized API available for simple, one-shot, delayed actions.

It consists of one function (available in the ``Machine`` class):

.. code-block:: csharp

    void ExecuteIn(Action what, TimeSpan when)

where ``what`` is the action to execute and ``when`` tells how much **virtual** time it should take before executing it.

.. note::

   When a zero timespan is supplied to the function, the action still won't be executed immediately.
   Instead it will be executed on the next synchronization point.
