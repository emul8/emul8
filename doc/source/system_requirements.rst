System requirements
===================

Emul8 runs in the portable *mono* framework. It was specifically tested on Mac and Ubuntu 14.04, but should also run on most of popular Linux distributions.

Prerequisites (Mac)
-------------------

The installation procedure on Mac is fairly straightforward, as you can use `an official Mono release <https://download.mono-project.com/archive/mdk-latest-stable.pkg>`_.

If not already present, install `homebrew <http://brew.sh/>`_ and then::

   brew install binutils gnu-sed coreutils homebrew/versions/gcc49 dialog

Some less frequently used features (*tun* networking, advanced logger etc.) will not yet work on a Mac, but Emul8 can easily be used without them.

Prerequisites (Linux)
---------------------

In order to run the Emul8 framework, *mono* 5.0 or newer is required. To get it, begin with the following commands::

   sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
   echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
   sudo apt-get update
   sudo apt-get install mono-complete

Before you start the build procedure make sure that the following components are installed in your system (Ubuntu/Debian package names are given in brackets):

* *autoconf* (``autoconf``)
* *automake* (``automake``)
* *mono* (``mono-complete``)
* *llvm* (``llvm``)
* *libtool* (``libtool``)
* *g++* (``g++``)
* *gksudo* (``gksu``)
* *libgtk* (``libgtk2.0-dev``)
* *dialog* (``dialog``)
* *screen* (``screen``)
* *realpath* (``realpath``)
* *tun* kernel module for guest-host networking
* *tunctl* (``uml-utilities``) to interact with *tun* module
* *xterm* or *gnome-terminal* or *putty* for window-based terminal analyzers
* *gtk-sharp2* (``gtk-sharp2``) for GTK2-based plugins

Instead of *gksudo* you may have *beesu* or *kdesudo*.

