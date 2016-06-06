//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.Linq;
using System;
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Elements.Projects;

namespace Emul8.Bootstrap
{
    public class UiStep : Step<RadiolistDialog>
    {
        public UiStep(PathHelper pathHelper) : base(null)
        {
            this.pathHelper = pathHelper;
            uiProjects = Scanner.Instance.Projects.OfType<UiProject>();
        }
        
        protected override RadiolistDialog CreateDialog()
        {
            var dialog = new RadiolistDialog(MainClass.Title, "Choose UI version:", uiProjects.Select(x => Tuple.Create(pathHelper.GetRelativePath(x.Path), x.Name)));
            dialog.SelectedKeys = selectedKeys;
            return dialog;
        }
        
        protected override void OnSuccess()
        {
           UIProject = uiProjects.Single(x => pathHelper.AreSame(x.Path, Dialog.SelectedKeys.First()));
           selectedKeys = new List<string>(Dialog.SelectedKeys);
        }
        
        public Project UIProject { get; private set; }
        
        private List<string> selectedKeys;
        private readonly PathHelper pathHelper;
        private readonly IEnumerable<UiProject> uiProjects;
    }
}

