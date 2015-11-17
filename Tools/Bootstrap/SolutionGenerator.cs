//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Collections.Generic;
using System.Linq;

namespace Emul8.Bootstrap
{
    public static class SolutionGenerator
    {
        public static Solution Generate(Project mainProject, IEnumerable<Project> additionalProjects)
        {
            var projects = 
                mainProject.GetAllReferences()
                .Union(additionalProjects.SelectMany(x => x.GetAllReferences()))
                .Union(additionalProjects);
            var motherProject = Project.CreateEntryProject(mainProject, projects);

            return new Solution(new [] { motherProject, mainProject }.Union(projects));
        }   
        
        public static Solution GenerateWithAllReferences(UiProject mainProject, IEnumerable<Project> extraProjects = null)
        {
            if(extraProjects == null)
            {
                extraProjects = new Project[0];
            }
            
            var extensionProjects = Scanner.Instance.Projects.OfType<ExtensionProject>();
            var pluginProjects = Scanner.Instance.Projects.OfType<PluginProject>().Where(x => !x.PluginModes.Any() || x.PluginModes.Contains(mainProject.UiType));
            
            var projects = extensionProjects
                .Union(extraProjects)
                .Union(extraProjects.SelectMany(x => x.GetAllReferences()))
                .Union(extensionProjects.SelectMany(x => x.GetAllReferences()))
                .Union(pluginProjects)
                .Union(pluginProjects.SelectMany(x => x.GetAllReferences())).ToList();
                    
            return SolutionGenerator.Generate(mainProject, projects);
        }
    }
}

