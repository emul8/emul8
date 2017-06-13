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
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Elements.Projects;

namespace Emul8.Bootstrap
{
    public class Scanner
    {
        static Scanner()
        {
            Instance = new Scanner();
        }
        
        public static Scanner Instance { get; private set; }
        
        public void ScanDirectory(string directory, IEnumerable<string> excludes = null)
        {
            try
            {
                foreach(var interestingElement in interestingElements)
                {
                    foreach(var file in Directory.EnumerateFiles(directory,string.Format("*.{0}", interestingElement.Key), SearchOption.AllDirectories))
                    {
                        if(excludes != null && excludes.Any(x => file.Contains(x)))
                        {
                            continue;
                        }
                        IInterestingElement element;
                        if(interestingElement.Value(Path.GetFullPath(file), out element))
                        {
                            elements.Add(element);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // just skip folders you have no access rights to
            }
        }
        
        public void ScanDirectories(IEnumerable<string> directories, IEnumerable<string> excludes = null)
        {
            foreach (var directory in directories)
            {
                ScanDirectory(directory, excludes);
            }
        }

        public IEnumerable<Project> GetProjectsOfType(ProjectType type)
        {
            switch(type)
            {
            case ProjectType.CpuCore:
                return elements.OfType<CpuCoreProject>();
            case ProjectType.Extension:
                return elements.OfType<ExtensionProject>();
            case ProjectType.Plugin:
                return elements.OfType<PluginProject>();
            case ProjectType.Tests:
                return elements.OfType<TestsProject>();
            case ProjectType.UI:
                return elements.OfType<UiProject>();
            default:
                throw new ArgumentException("Unsupported project type");
            }
        }

        public IEnumerable<IInterestingElement> Elements { get { return elements; } }

        private Scanner()
        {
            elements = new HashSet<IInterestingElement>();
        }

        private readonly HashSet<IInterestingElement> elements;

        private static Dictionary<string, TryCreateElementDelegate> interestingElements = new Dictionary<string, TryCreateElementDelegate>
        {
            { "csproj", Project.TryLoadFromFile },
            { "robot", RobotTestSuite.TryCreate }
        };
    }

    public delegate bool TryCreateElementDelegate(string path, out IInterestingElement result);
}

