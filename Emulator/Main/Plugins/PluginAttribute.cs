//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities;

namespace Emul8.Plugins
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute, IInterestingType
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Vendor { get; set; }
        public Type[] Dependencies { get; set; }
        public string[] Modes { get; set; }
    }
}

