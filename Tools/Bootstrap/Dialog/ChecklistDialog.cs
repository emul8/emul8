//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

namespace Emul8.Bootstrap
{
    public class ChecklistDialog : ListDialog
    {
        public ChecklistDialog(string title, string message, IEnumerable<Tuple<string, string>> options) : base("checklist", title, message, true, options)
        {
        }
        
        public override DialogResult Show()
        {
            bool? selectAll = null;
            DialogResult result;
            do
            {
                var configurationString = new StringBuilder(); 
                configurationString.AppendFormat("--extra-button --extra-label \"{5}\" --backtitle \"{0}\" --{4} \"{1}\" 0 0 {2} {3}", title, message, options.Count(), GenerateDialogOptions(selectAll), type, (!selectAll.HasValue || !selectAll.Value) ? "Select all" : "Deselect all");
                if(ShowBackButton)
                {
                    configurationString.Insert(0, "--help-button --help-label \"Back\" ");
                }
                result = Show(configurationString.ToString());
                if(result == DialogResult.SelectDeselectAll)
                {
                    // select / deselect all
                    selectAll = (selectAll == null) ? true : !selectAll;
                }
            }
            while(result == DialogResult.SelectDeselectAll);
            
            if(result == DialogResult.Ok)
            {
                SelectedKeys = output.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            
            return result;
        }
        
        public bool ShowBackButton { get; set; }
    }
}

