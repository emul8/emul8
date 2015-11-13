//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emul8.Launcher
{
    public static class Scanner
    {
        public static IEnumerable<LaunchDescriptorsGroup> ScanForInterestingBinaries(string directory)
        {
            var descriptors = new List<LaunchDescriptor>();
            foreach(var file in Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories))
            {
                LaunchDescriptor descriptor;
                if(LaunchDescriptor.TryReadFromAssembly(file, out descriptor))
                {
                    descriptors.Add(descriptor);
                }
            }

            var result = new List<LaunchDescriptorsGroup>();
            var groups = descriptors.GroupBy(x => new { x.Name, x.Description, x.Priority, x.ShortSwitch, x.LongSwitch, x.ProvidesHelp});
            foreach(var group in groups)
            {
                result.Add(new LaunchDescriptorsGroup(group.ToArray()));
            }

            return result;
        }
    }
}
