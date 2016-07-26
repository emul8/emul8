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
    public class PluginStep : ProjectsListStep<PluginProject>
    {
        public PluginStep(string message, PathHelper pathHelper) : base(message, pathHelper)
        {
        }
        
        protected override bool ShouldBeShown(StepManager m)
        {
            var additionalProjects = m.GetPreviousSteps<ProjectsListStep>(this).SelectMany(x => x.AdditionalProjects).Union(m.GetStep<UiStep>().UIProject.GetAllReferences()).ToList();
            var uiProject = m.GetPreviousSteps<UiStep>(this).First().UIProject;
            ScannedProjects = new HashSet<Project>(Scanner.Instance.Elements.OfType<PluginProject>().Where(x => !x.PluginModes.Any() || x.PluginModes.Contains(uiProject.Name)));
            ScannedProjects.ExceptWith(additionalProjects);
            return ScannedProjects.Any();
        }
    }
}

