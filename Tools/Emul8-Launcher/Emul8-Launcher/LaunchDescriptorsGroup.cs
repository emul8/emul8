//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Antmicro.OptionsParser;
using System.Linq;

namespace Emul8.Launcher
{
    public class LaunchDescriptorsGroup
    {
        public LaunchDescriptorsGroup(LaunchDescriptor[] descriptors)
        {
            if(descriptors == null || descriptors.Length == 0)
            {
                throw new ArgumentException();
            }

            this.descriptors = descriptors;
        }

        public LaunchDescriptor ForConfiguration(LaunchDescriptor.Configuration configuration)
        {
            return descriptors.SingleOrDefault(x => x.AssemblyConfiguration == configuration);
        }

        public void GenerateSwitches(OptionsParser optionsParser)
        {
            // here we assume that all descriptors from `descriptors` array
            // have the same basic attributes (name, description, switches, etc.)
            // they should differ only in configuration and path
            SwitchOption = optionsParser.WithOption<bool>(descriptors[0].ShortSwitch ?? Tokenizer.NullCharacter, descriptors[0].LongSwitch);
            SwitchOption.Description = descriptors[0].Description;

            if(descriptors[0].ProvidesHelp)
            {
                HelpOption = optionsParser.WithOption<bool>(string.Format("help-{0}", descriptors[0].LongSwitch));
                HelpOption.Description = string.Format("Show help for {0}", descriptors[0].Name);
            }
        }

        public string Name { get { return descriptors[0].Name; } }
        public uint Priority { get { return descriptors[0].Priority; } }

        public CommandLineOption<bool> SwitchOption { get; private set; }
        public CommandLineOption<bool> HelpOption { get; private set; }

        private readonly LaunchDescriptor[] descriptors;
    }
}
