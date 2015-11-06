Introduction
============

This document is intended as a user manual for `Emul8 <http://emul8.org>`_, the extensible open source embedded systems emulation framework.
Using Emul8, you can write, debug and test sofware for embedded boards (or whole systems, like a set of sensor nodes connected to a data gateway) and run it in a virtual environment on your PC, without the need to touch the physical hardware. Indeed, the hardware you are programming for may not even exist yet.

This first several chapters of this manual describe the framework from a typical user's perspective, but several more chapters follow that will helpful to developers and power users.

If you are a developer and would like to contribute to Emul8 as such, (or just want to report a bug or propose a feature), please refer to the `contribution guidelines <https://github.com/emul8/emul8/blob/master/CONTRIBUTING.rst>`_.

Structure of this manual
------------------------

This introduction is meant to describe the Emul8 framework and clarify some of the :ref:`basic concepts <terms>` behind it.

In the section entitled :doc:`system_requirements` you will find the dependencies and steps necessary to set up your system and prepare your environment.

Section :doc:`build_procedure` describes in detail the process of compiling Emul8.

:doc:`running` shows how to start Emul8 in different modes and describes Emul8's CLI, the :term:`Monitor`.

:doc:`emulation_files` covers two different types of scripts used to prepare an emulation from scratch.

:doc:`workflow` shows how to start an emulation, describing one of our demo scripts.

:doc:`time_and_synchronization` presents a more advanced topic of execution determinsm.

:doc:`peripherals` starts the part focused on model development, describing the process of peripheral implementation.

:doc:`cpu_registers` presents the framework used to generate the interface to CPU registers.

:doc:`threads` describes the mechanisms used to run code in separate threads while still preserving the execution determinism.

:doc:`synchronization_management` dives deeper to determinism, showing how to synchronize connected machines and external devices.

:doc:`plugins` is an overview of the plugin system.

:doc:`native_managed_bindings` describes how to connect the Emul8 framework to system-native libraries.

.. _terms:

Basic terms
-----------

.. glossary::

   <emul8root>
      the root directory where Emul8 is installed

   Monitor
      the Emul8 CLI (command line interface)

   emulation
      the entire virtual world created by emul8 which may include one or more :term:`machines <machine>` as well as helper emulation objects

   machine
      an instance of a :term:`platform`, a virtual representation of a physical embedded board

   peripheral
      any block constituting part of a :term:`machine`, like a UART controller or memory chip

   platform
      a real-world embedded board, whose virtual copy in emul8 is called :term:`machine`
