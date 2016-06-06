//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
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
        
        public void ScanDirectory(string directory)
        {
            try
            {
                foreach(var file in Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories))
                {
                    Project project;
                    if(Project.TryLoadFromFile(Path.GetFullPath(file), out project))
                    {
                        Projects.Add(project);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // just skip folders you have no access rights to
            }
        }
        
        public void ScanDirectories(IEnumerable<string> directories)
        {
            foreach (var directory in directories)
            {
                ScanDirectory(directory);
            }
        }

        public IEnumerable<Project> GetProjectsOfType(ProjectType type)
        {
            
            throw new NotImplementedException();
        }
        
        private Scanner()
        {
            Projects = new HashSet<Project>();
        }
        
        public HashSet<Project> Projects { get; private set; }
    }
}

