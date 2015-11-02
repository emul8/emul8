//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using System.Reflection;
using System.Linq;
using Emul8.Core;

namespace Emul8.UserInterface.Commands
{
    public class VersionCommand : AutoLoadCommand
    {
        [Runnable]
        public void Run(ICommandInteraction writer)
        {         
            writer.WriteLine(VersionString);
        }

        public static String VersionString
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(Machine));
                var name = ((AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
                var version = assembly.GetName().Version;
                var gitVersion = ((AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion;
                var copyright = ((AssemblyCopyrightAttribute)assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
                return string.Format("{0}, version {1} ({2})\r\nCopyright {3}", name, version, gitVersion, copyright.Replace("\n", "\r\n"));
            }
        }

        public VersionCommand(Monitor monitor) : base(monitor, "version", "shows version information.")
        {
        }
    }
}

