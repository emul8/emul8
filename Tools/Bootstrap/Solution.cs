//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Elements.Projects;

namespace Emul8.Bootstrap
{
    public class Solution
    {
        public Solution(string name, IEnumerable<Project> projects)
        {
            Name = name;
            this.projects = projects;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine();
            builder.AppendLine("Microsoft Visual Studio Solution File, Format Version 11.00");
            builder.AppendLine("# Visual Studio 2010");

            foreach(var project in projects)
            {
                builder.AppendFormat("Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{0}\", \"{1}\", \"{{{2}}}\"\n", project.Name, project.Path, project.GUID.ToString().ToUpper());
                builder.AppendLine("EndProject");
            }

            var typeToGuidMap = new Dictionary<string, Tuple<Guid, IEnumerable<Project>>>();

            var pluginProjects = projects.OfType<PluginProject>().AsEnumerable<Project>();
            if(pluginProjects.Any())
            {
                typeToGuidMap.Add("Plugins", Tuple.Create(Guid.NewGuid(), pluginProjects));
            }

            var testsProjects = projects.OfType<TestsProject>().AsEnumerable<Project>();
            if(testsProjects.Any())
            {
                typeToGuidMap.Add("Tests", Tuple.Create(Guid.NewGuid(), testsProjects));
            }

            var coresProjects = projects.OfType<CpuCoreProject>();
            if(coresProjects.Any())
            {
                typeToGuidMap.Add("Cores", Tuple.Create(Guid.NewGuid(), coresProjects.Cast<Project>()));
            }

            var extensionProjects = projects.OfType<ExtensionProject>().AsEnumerable<Project>();
            if(extensionProjects.Any())
            {
                typeToGuidMap.Add("Extensions", Tuple.Create(Guid.NewGuid(), extensionProjects));
            }

            var librariesProjects = projects.Where(x => x.Path.Contains("/External/"));
            if(librariesProjects.Any())
            {
                typeToGuidMap.Add("Libraries", Tuple.Create(Guid.NewGuid(), librariesProjects));
            }

            foreach(var type in typeToGuidMap)
            {
                builder.AppendFormat("Project(\"{{2150E333-8FDC-42A3-9474-1A3956D46DE8}}\") = \"{0}\", \"{0}\", \"{{{1}}}\"\n", type.Key, type.Value.Item1.ToString().ToUpper());
                builder.AppendLine("EndProject");
            }

            builder.AppendLine("Global");
            builder.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            builder.AppendLine("\t\tDebug|x86 = Debug|x86");
            builder.AppendLine("\t\tRelease|x86 = Release|x86");
            builder.AppendLine("\tEndGlobalSection");
            builder.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

            foreach(var project in projects)
            {
                var guid = project.GUID.ToString().ToUpper();
                builder.AppendFormat("\t\t\t{{{0}}}.Debug|x86.ActiveCfg = Debug|{1}\n", guid, project.Target);
                builder.AppendFormat("\t\t\t{{{0}}}.Debug|x86.Build.0 = Debug|{1}\n", guid, project.Target);
                builder.AppendFormat("\t\t\t{{{0}}}.Release|x86.ActiveCfg = Release|{1}\n", guid, project.Target);
                builder.AppendFormat("\t\t\t{{{0}}}.Release|x86.Build.0 = Release|{1}\n", guid, project.Target);
            }

            builder.AppendLine("\tEndGlobalSection");

            builder.AppendLine("\tGlobalSection(NestedProjects) = preSolution");

            foreach(var type in typeToGuidMap)
            {
                foreach(var project in type.Value.Item2)
                {
                    builder.AppendFormat("\t\t\t{{{0}}} = {{{1}}}\n", project.GUID.ToString().ToUpper(), type.Value.Item1);
                }
            }

            builder.AppendLine("\tEndGlobalSection");

            builder.AppendLine("EndGlobal");

            return builder.ToString();
        }

        public IEnumerable<Project> Projects
        {
            get
            {
                return projects; 
            }
        }

        public string Name { get; private set; }

        private static IEnumerable<Project> GenerateAllReferences(IEnumerable<Project> projects)
        {
            var result = new HashSet<Project>();
            foreach(var project in projects)
            {
                GenerateReferencesInner(project, result);
            }
            return result;
        }

        private static void GenerateReferencesInner(Project project, HashSet<Project> projects)
        {
            if(!projects.Contains(project))
            {
                projects.Add(project);

                foreach(var reference in project.GetAllReferences())
                {
                    GenerateReferencesInner(reference, projects);
                }
            }
        }

        private readonly IEnumerable<Project> projects;
    }
}

