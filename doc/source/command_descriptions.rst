.. glossary::

   allowPrivates
      allow private fields and properties manipulation.

      short: **privs**

   analyzers
      shows available analyzers for peripheral.

   createPlatform
      creates a platform.

      short: **c**

   execute
      executes a command or the content of a variable.

   halt
      stops the emulation.

      short: **h**

   help
      prints this help message or info about specified command.

      short: **?**

   include
      loads a monitor script, python code or a plugin class.

      short: **i**

   log
      logs messages.

   logFile
      sets the output file for logger.

      short: **logF**

   logLevel
      sets logging level for backends.

      ===== =======
      Level Name
      ===== =======
      -1    NOISY
      0     DEBUG
      1     INFO
      2     WARNING
      3     ERROR
      ===== =======

   mach
      list and manipulate machines available in the environment.

      **mach set** <name>
         Enable the given machine.

      **mach add** <name>
         Create a new machine with the given name.

      **mach rem** <name>
         Remove a machine.

      **mach create**
         Create a new machine with a generic name and switch to it.

      **mach clear**
         Clear the current selection.

   macro
      sets a macro.

   numbersMode
      sets the way numbers are displayed.

      Options:

      * Hexadecimal
      * Decimal
      * Both

   path
      allows modification of internal 'PATH' variable.

      **path set** <PATH>
         Set ``PATH`` to the given value.

      **path add** <PATH>
         Append the given value to ``PATH``.

      **path reset**
         Reset ``PATH`` to it's default value.

   peripherals
      prints list of registered and named peripherals.

      short: **peri**

   python
      executes the provided python command.

      short: **py**

   quit
      quits the emulator.

      short: **q**

   require
      verifies the existence of a variable.

   runMacro
      executes a command or the content of a macro.

   set
      sets a variable.

   showAnalyzer
      opens a peripheral backend analyzer.

      short: **sa**

   start
      starts the emulation.

      short: **s**

      **start <PATH>**
         just like :term:`include \<PATH\> <include>`, but also start all machines created in the script.

   string
      treat given arguments as a single string.

      short: **str**

   using
      expose a prefix to avoid typing full object names.

      **using -**
         Clear all previous **using** calls

      Example: ``using sysbus.gpioPortA``

   verboseMode
      controls the verbosity of the Monitor.

   version
      shows version information.

