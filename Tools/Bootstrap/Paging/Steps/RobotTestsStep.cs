//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emul8.Bootstrap.Elements;

namespace Emul8.Bootstrap.Paging.Steps
{
    public class RobotTestsStep : Step<ChecklistDialog>
    {
        public RobotTestsStep(string message, PathHelper pathHelper) : base(message)
        {
            this.pathHelper = pathHelper;
        }

        public IEnumerable<RobotTestSuite> SelectedTests { get; private set; }

        protected override ChecklistDialog CreateDialog()
        {
            var dialog = new ChecklistDialog(MainClass.Title, message, availableTests.Select(x => Tuple.Create(pathHelper.GetRelativePath(x.Path), Path.GetFileNameWithoutExtension(x.Path)))) 
            { 
                ShowBackButton = true 
            };
            dialog.SelectedKeys = selectedKeys;
            return dialog;
        }

        protected override bool ShouldBeShown(StepManager m)
        {
            availableTests = Scanner.Instance.Elements.OfType<RobotTestSuite>();
            return availableTests.Any();
        }

        protected override void OnSuccess()
        {
            var selectedTests = new HashSet<RobotTestSuite>();
            selectedKeys = new List<string>(Dialog.SelectedKeys);
            foreach(var key in selectedKeys)
            {
                selectedTests.Add(new RobotTestSuite(key));
            }

            SelectedTests = selectedTests;
        }

        private PathHelper pathHelper;
        private List<string> selectedKeys;
        private IEnumerable<RobotTestSuite> availableTests;
    }
}

