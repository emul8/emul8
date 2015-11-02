//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Utilities
{
    public class DemosParser
    {
        public DemosParser(string baseDirectory)
        {
            this.baseDirectory = baseDirectory;
            Demos = SearchForDemosInternal(new DirectoryInfo(baseDirectory));
        }

        public IEnumerable<Tuple<string[], DemoDetail[]>> GetDemosGroupedByFolder()
        {
            return Demos
                .GroupBy(
                    x => new DirectoryPath(baseDirectory, Path.GetDirectoryName(x.Path)), 
                    y => y)
                .OrderBy(x => x.Key.SplitPath.Length)
                .Select(x => Tuple.Create(x.Key.SplitPath, x.ToArray()));
        }

        public IEnumerable<DemoDetail> Demos { get; private set; }

        public const string DemosPath = "scripts/demos";

        private static DemoDetail ReadDetails(string path)
        {
            if (path == null)
            {
                return null;
            }

            var name = string.Empty;
            var icon = string.Empty;
            var description = "No description provided.";

            var tagParser = new PropertyTagParser(File.ReadAllLines(path));
            Tuple<string, string> tag;
            var flag = false;
            while((tag = tagParser.GetNextTag()) != null)
            {
                switch(tag.Item1)
                {
                case "name":
                    name = tag.Item2;
                    flag = true;
                    break;
                case "icon":
                    icon = tag.Item2;
                    break;
                case "description":
                    description = tag.Item2;
                    break;
                }
            }

            return flag ? new DemoDetail { FileName = Path.GetFileName(path), Path = path, Name = name, Icon = icon, Description = description } : null;
        }

        private List<DemoDetail> SearchForDemosInternal(DirectoryInfo dir)
        {
            var result = new List<DemoDetail>();

            var subdirs = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var subdir in subdirs.Reverse())
            {
                result.AddRange(SearchForDemosInternal(subdir));
            }

            foreach (var file in dir.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                if(file.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    continue;
                }

                var details = ReadDetails(file.FullName);
                if(details != null)
                {
                    result.Add(details);
                }
            }

            return result;
        }

        private string baseDirectory;

        public class DemoDetail
        {
            public string FileName {get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Description { get; set; }
        }

        private class DirectoryPath 
        {
            public DirectoryPath(string basePath, string path)
            {
                var baseUri = new Uri(Path.GetDirectoryName(Path.GetFullPath(basePath + Path.VolumeSeparatorChar)) + Path.VolumeSeparatorChar);
                var relativeUri = baseUri.MakeRelativeUri(new Uri(path));

                SplitPath = relativeUri.ToString().Split(Path.VolumeSeparatorChar);
            }

            public string[] SplitPath { get; private set; }

            public override bool Equals(object obj)
            {
                var otherDirectoryPath = obj as DirectoryPath;
                if(otherDirectoryPath == null)
                {
                    return false;
                }

                return SplitPath.SequenceEqual(otherDirectoryPath.SplitPath);
            }

            public override int GetHashCode()
            {
                return SplitPath.GetHashCode();
            }
        }
    }
}

