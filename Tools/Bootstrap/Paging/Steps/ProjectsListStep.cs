//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Bootstrap
{
    public abstract class ProjectsListStep : Step<ChecklistDialog>
    {
        public IEnumerable<Project> AdditionalProjects { get; protected set; }
        
        protected ProjectsListStep()
        {
            AdditionalProjects = new Project[0];
        }
    }
    
    public class ProjectsListStep<T> : ProjectsListStep where T : Project
    {
        public ProjectsListStep(string message, PathHelper pathHelper)
        {
            this.message = message;
            this.pathHelper = pathHelper;
        }
        
        protected override ChecklistDialog CreateDialog()
        {
            var dialog = new ChecklistDialog(MainClass.Title, message, ScannedProjects.Select(x => Tuple.Create(pathHelper.GetRelativePath(x.Path), x.Name))) { ShowBackButton = true };
            dialog.SelectedKeys = selectedKeys;
            return dialog;
        }
        
        protected override bool ShouldBeShown(StepManager m)
        {
            var additionalProjects = m.GetPreviousSteps<ProjectsListStep>(this).SelectMany(x => x.AdditionalProjects).Union(m.GetStep<UiStep>().UIProject.GetAllReferences()).ToList();
            ScannedProjects = new HashSet<Project>(Scanner.Instance.Projects.OfType<T>());
            ScannedProjects.ExceptWith(additionalProjects);
            return ScannedProjects.Any();
        }
        
        protected override void OnSuccess()
        {
            var additionalProjects = new HashSet<Project>();
            selectedKeys = new List<string>(Dialog.SelectedKeys);
            foreach(var key in selectedKeys)
            {
                var projectToAdd = ScannedProjects.Single(x => pathHelper.AreSame(x.Path, key));
                additionalProjects.Add(projectToAdd);
                foreach(var reference in projectToAdd.GetAllReferences())
                {
                    additionalProjects.Add(reference);
                }
            }
                    
            AdditionalProjects = additionalProjects;
        }
        
        protected HashSet<Project> ScannedProjects;
        
        private List<string> selectedKeys;
        private readonly string message;
        private readonly PathHelper pathHelper;
    }
}

