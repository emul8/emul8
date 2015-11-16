//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Mono.Cecil;
using System.Linq;
using System.Reflection;
using Emul8.LaunchAttributes;

namespace Emul8.Launcher
{
    public class LaunchDescriptor
    {
        public static bool TryReadFromAssembly(string assemblyPath, out LaunchDescriptor descriptor)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

            var nameAttribute = assembly.SingleCustomAttributeOfType<Emul8.LaunchAttributes.NameAttribute>();
            var descriptionAttribute = assembly.SingleCustomAttributeOfType<Emul8.LaunchAttributes.DescriptionAttribute>();
            var priorityAttribute = assembly.SingleCustomAttributeOfType<PriorityAttribute>();
            var switchAttribute = assembly.SingleCustomAttributeOfType<SwitchAttribute>();
            var configurationAttribute = assembly.SingleCustomAttributeOfType<AssemblyConfigurationAttribute>();

            if(nameAttribute == null
                || descriptionAttribute == null
                || priorityAttribute == null
                || switchAttribute == null
                || configurationAttribute == null)
            {
                descriptor = null;
                return false;
            }

            descriptor = new LaunchDescriptor
            {
                Path = assemblyPath,
                Name = (string)nameAttribute.ConstructorArguments[0].Value,
                Description = (string)descriptionAttribute.ConstructorArguments[0].Value,
                Priority = (uint)priorityAttribute.ConstructorArguments[0].Value,
                ShortSwitch = switchAttribute.ConstructorArguments.Count == 2 ? (char?)switchAttribute.ConstructorArguments[1].Value : null,
                LongSwitch = (string)switchAttribute.ConstructorArguments[0].Value,
                ProvidesHelp = assembly.SingleCustomAttributeOfType<ProvidesHelpAttribute>() != null,
                AssemblyConfiguration = (Configuration)Enum.Parse(typeof(Configuration), (string)configurationAttribute.ConstructorArguments[0].Value)
            };
            return true;
        }

        private LaunchDescriptor()
        {
        }

        public string Path { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public uint Priority { get; private set; }
        public char? ShortSwitch { get; private set; }
        public string LongSwitch { get; private set; }
        public bool ProvidesHelp { get; private set; }
        public Configuration AssemblyConfiguration { get; private set; }

        public enum Configuration
        {
            Debug,
            Release
        }
    }

    static class AssemblyDefinitionExtensions
    {
        public static CustomAttribute SingleCustomAttributeOfType<T>(this AssemblyDefinition assembly)
        {
            return assembly.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == typeof(T).FullName);
        }
    }
}
