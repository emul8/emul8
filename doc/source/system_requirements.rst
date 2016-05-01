System requirements
===================

Emul8 runs in the portable *mono* framework. It was specifically tested on Mac and Ubuntu 14.04, but should also run on most of popular Linux distributions.

Prerequisites (Mac)
-------------------

The installation procedure on Mac is fairly straightforward, as you can use `an official 4.2.3 Mono release <http://download.mono-project.com/archive/4.2.3/macos-10-x86/MonoFramework-MDK-4.2.3.4.macos10.xamarin.x86.pkg>`_.

If not already present, install `homebrew <http://brew.sh/>`_ and then:: 

   brew install binutils gnu-sed coreutils homebrew/versions/gcc49 dialog

Some less frequently used features (*tun* networking, advanced logger etc.) will not yet work on a Mac, but Emul8 can easily be used without them. 

Prerequisites (Linux)
---------------------

In order to run the Emul8 framework, a specific version of *mono* is required.
As we use a development version from the mainline *mono* git repository, it is necessary to build it from source.

Our current fixed version of *mono* is: **37f2657ac778d3934a1b3245076849816ff9e6f9**.

Before you start the build procedure make sure that the following components are installed in your system (Ubuntu/Debian package names are given in brackets):

* *autoconf* (``autoconf``)
* *automake* (``automake``)
* *mono* (``version 37f2657ac778d3934a1b3245076849816ff9e6f9``)
* *llvm* (``llvm``)
* *libtool* (``libtool``)
* *g++* (``g++``)
* *vte-sharp* (``libvte0.16-cil-dev``)
* *gksudo* (``gksu``)
* *libgtk* (``libgtk2.0-dev``)
* *dialog* (``dialog``)
* *screen* (``screen``)
* *tun* kernel module for guest-host networking
* *xterm* or *gnome-terminal* or *putty* for window-based terminal analyzers

Instead of *gksudo* you may have *beesu* or *kdesudo*.

Guidelines for building *mono* (Linux)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

.. note::

   As part of the *mono* framework is written in *C#*, it is required to have a *C#* compiler already installed when compiling *mono* from source.
   The recommended procedure is to install the *mono* package available in your distribution and overwrite it with a compiled version.

#. Make sure you have *mono* (preferably from packages) already installed in your system and check its location:

   ``whereis mono``

#. Clone the *mono* repository from *github* using:

   ``git clone https://github.com/mono/mono``

#. Change current directory to *mono*:

   ``cd mono``

#. Checkout to our fixed commit using:

   ``git checkout 37f2657ac778d3934a1b3245076849816ff9e6f9``

#. Configure sources with your current *mono* installation location, using a prefix value equal to the part of the path before ``/bin/mono``, e.g. if ``whereis mono`` returned ``/usr/bin/mono`` then your prefix location is ``/usr``:

   ``./autogen.sh --prefix=/usr``

   .. note::

      The configuration process uses external tools: ``autoconf``, ``libtool``, ``automake``, ``g++`` so make sure you have them installed on your system.

#. Build:

   ``make -j9``

   .. note::

      The ``-j9`` switch means that ``make`` can be run in parallel in up to 9 processes. Change this value according to your hardware capabilities.

#. Install using a *root* account:

   ``sudo make install``

   .. warning::

      After updating packages in your system make sure that *mono* was not overwritten by an older version. If so, please reinstall by repeating this step.

#. Verify your installation:

   ``mono --version``

   If the procedure was successfully executed, you should see something like:

   .. code-block:: none

	Mono JIT compiler version 4.3.0 (explicit/37f2657 śro, 25 lis 2015, 17:10:07 CET)
	Copyright (C) 2002-2014 Novell, Inc, Xamarin Inc and Contributors. www.mono-project.com
		TLS:           __thread
		SIGSEGV:       altstack
		Notifications: epoll
		Architecture:  amd64
		Disabled:      none
		Misc:          softdebug 
		LLVM:          supported, not enabled.
		GC:            sgen


