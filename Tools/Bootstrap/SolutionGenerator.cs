//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Linq;
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Elements.Projects;

namespace Emul8.Bootstrap
{
    public static class SolutionGenerator
    {
        public static Solution Generate(Project mainProject, bool generateEntryProject, string outputPath, IEnumerable<Project> additionalProjects)
        {
            var projects = 
                new[] { mainProject }.Union(
                mainProject.GetAllReferences()
                .Union(additionalProjects.SelectMany(x => x.GetAllReferences()))
                .Union(additionalProjects)).ToList();

            if(generateEntryProject)
            {
                var motherProject = Project.CreateEntryProject(mainProject, outputPath, projects.Skip(1));
                projects.Insert(0, motherProject);
            }

            return new Solution(projects);
        }   
        
        public static Solution GenerateWithAllReferences(UiProject mainProject, bool generateEntryProject, string binariesPath, IEnumerable<Project> extraProjects = null)
        {
            if(extraProjects == null)
            {
                extraProjects = new Project[0];
            }
            
            var extensionProjects = Scanner.Instance.Elements.OfType<ExtensionProject>();
            var pluginProjects = Scanner.Instance.Elements.OfType<PluginProject>().Where(x => !x.PluginModes.Any() || x.PluginModes.Contains(mainProject.UiType));
            
            var projects = extensionProjects
                .Union(extraProjects)
                .Union(extraProjects.SelectMany(x => x.GetAllReferences()))
                .Union(extensionProjects.SelectMany(x => x.GetAllReferences()))
                .Union(pluginProjects)
                .Union(pluginProjects.SelectMany(x => x.GetAllReferences())).ToList();
                    
            return SolutionGenerator.Generate(mainProject, generateEntryProject, binariesPath, projects);
        }
    }
}

