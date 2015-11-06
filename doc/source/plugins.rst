Plugins and Extensions
======================

The *Emul8* framework allows to dynamically add new components from external DLL files.
The ``TypeManager`` class is responsible for scanning assemblies, detecting **interesting types** and resolving them if needed - loading them into .NET framework along with dependencies.
Due to the use of the *Cecil* library, no class is loaded into the .NET *AppDomain* until it is necessary.

.. note::

   A type is considered interesting if it implements our custom empty ``IInterestingType`` interface or has at least one attribute implementing it.

An assembly containing interesting types is referenced to as **extension library**.
It can provide e.g. models of new peripherals or commands for the *Monitor*.

**Plugins** are a special kind of extensions that can be enabled and disabled by the user.
When a *plugin* is enabled, the framework creates an object for the plugin and stores a reference to the object in ``PluginManager``.

When it is disabled, the object is disposed (assuming that the plugin class implements ``IDisposable``) and the reference is forgotten.
As a result, it is guaranteed that plugin object is not garbage-collected when being enabled.

Plugin initialization code must be kept in a constructor and optional clean-up code should be put in a ``Dispose`` method.
In order to make the ``TypeManager`` aware that a class implements the plugin functionality, the class must be decorated with a special ``PluginAttribute`` attribute.

``PluginAttribute`` provides the following properties used to identify a plugin:

* *name* - unique identifier of the plugin used in *Monitor* to reference this plugin,
* *description* - short description of the plugin describing functionality it provides,
* *version* - version of the plugin allowing to distinguish between different implementations of the same functionality,
* *vendor* - name of the unit responsible for creating the plugin,
* *dependencies* - list of plugins required to be loaded, as a list of types with ``PluginAttribute`` defined,
* *mode* - information in what mode the plugin can be run. Currently only the CLI mode is available.

A constructor of a plugin must either be parameterless, or consist only of certain allowed parameters.
As for now, the only parameter type allowed is ``Monitor``.

A plugin may access public APIs of *Emul8*, available either via the *Monitor* or many singleton/static classes.

To simplify the process of plugin development, a sample plugin has been implemented to serve programmers as a base for their own work.

The sample plugin is named ``SampleCommandPlugin`` and adds a new command to the *Monitor*.
Please note that you can use an ``AutoLoadCommand`` and avoid manual registration, but this does not allow to unload the plugin later.

Plugin management
-----------------

In order to make plugins visible to the framework, an extension library must be stored in the same folder as the *Emul8* binary.

In the *Monitor*, there is a special **plugins** object allowing to access and manage plugins. Three actions that can be called on it are:

* **EnablePlugin**
* **DisablePlugin**
* **GetPlugins**

.. note::

   Keep in mind that disabling a plugin does not unload the assembly from runtime as it is not supported by the framework.
