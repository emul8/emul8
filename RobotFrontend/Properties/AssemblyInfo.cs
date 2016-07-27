//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle ("Emul8 Robot frontend")]
#if (DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Antmicro")]
[assembly: AssemblyProduct("Emul8")]
[assembly: AssemblyCopyright("(c) Antmicro")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion ("1.0.*")]
