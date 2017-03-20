Emul8
=====

What is Emul8?
--------------

Emul8, as the name suggests, is an emulator of various embedded systems.
With Emul8 you can develop embedded software entirely in a virtual environment that runs within your PC.

If that still doesn't tell you much, visit the `Emul8 website <http://emul8.org/learn-more>`_ to learn more.

Supported architectures
-----------------------

* ARM Cortex-A and Cortex-M
* SPARC
* PowerPC
* x86 (experimental)

Installation
------------

Prerequisites (Mac)
+++++++++++++++++++

The installation procedure on Mac is fairly straightforward, as you can use `an official 4.2.3 Mono release <http://download.mono-project.com/archive/4.2.3/macos-10-x86/MonoFramework-MDK-4.2.3.4.macos10.xamarin.x86.pkg>`_.

If not already present, install `homebrew <http://brew.sh/>`_ and then::

   brew install binutils gnu-sed coreutils homebrew/versions/gcc49 dialog

Prerequisites (Linux)
+++++++++++++++++++++

The package names for prerequisites are given for Ubuntu 14.04 (please adjust those w/r to your distribution and version)::

   sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
   echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
   sudo apt-get update
   sudo apt-get install git mono-complete automake autoconf libtool g++ realpath \
                        gksu libgtk2.0-dev dialog screen uml-utilities gtk-sharp2

.. note::

   Emul8 requires mono version 4.6 or newer.

If you want to modify or extend the source code of Emul8, it is recommended to install the *MonoDevelop* IDE.

For a detailed information on the required packages, please consult the *System requirements* section in the documentation.

Getting the source
++++++++++++++++++

The Emul8 source code is available from GitHub::

   git clone https://github.com/emul8/emul8.git

No need to init any submodules - this will be done automatically at a later stage.

Bootstrapping Emul8
+++++++++++++++++++

Since Emul8 can be used for various purposes and not everyone needs all the modules and platforms, a simple bootstrapping system was created to make it easy to select the elements you need.

In order to create a solution file integrating the selected projects (*.csproj* files), use the *Bootstrap* tool::

   ./bootstrap.sh

and follow the instructions displayed on the screen.
As a result a *target* directory with an *Emul8.sln* file will be created, ready to be built.

.. note::

   If you just want to build everything, or for scripting purposes, instead of running in interactive mode, you can generate a solution containing all projects by executing::

      ./bootstrap.sh -a

Building Emul8
++++++++++++++

After bootstrapping the configuration (i.e., when an *Emul8.sln* file is created) it is possible to build it using::

   ./build.sh

with optional flags::

   -v          verbose mode
   -d          debug mode
   -c          clean instead of building
   -i          install (create a symbolic link in the */usr/local/bin directory*)
               so that Emul8 is available system-wide as *emul8*
   -p          create deb/rpm/arch packages

Running Emul8
-------------

In order to run Emul8 use::

   ./run.sh [file]

with optional flags::

   -d            debug mode
   -e COMMAND    execute command on startup (does not allow the [file] argument)
   -p            remove steering codes (e.g., colours) from output
   -P PORT       listen on a port for monitor commands instead of opening a window
   -h            help & usage

where ``[file]`` is an optional argument - path to a script that should be loaded on startup.

If you installed Emul8 with ``./build.sh -i``, you can use the system-wide command ``emul8`` with the same options.

Documentation
-------------

The source of the documentation, available in compiled form on `Read The Docs <https://emul8.readthedocs.org/en/latest/>`_, is located in the *doc* folder.
It is written in Sphinx, which can be installed as follows::

   sudo apt-get install python-pip
   sudo pip install sphinx

To compile the documentation, use::

   make html     # build HTML output

Or::

   make latexpdf # build PDF output, also requires LaTeX

License
-------

Emul8 is released under the permissive MIT license.
For details, See the *LICENSE* file.

Contributing
------------

Contributions can be made using the GitHub pull requests mechanism and are very welcome!
For details, see the *CONTRIBUTING* file.


