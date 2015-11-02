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
    public abstract class ListDialog : Dialog
    {
        protected ListDialog(string type, string title, string message, bool selectable, IEnumerable<Tuple<string, string>> options) : base(title, message)
        {
            this.options = options;
            this.type = type;
            this.selectableValues = selectable;
        }

        public override DialogResult Show()
        {
            var result = Show(string.Format("--backtitle \"{0}\" --{4} \"{1}\" 0 0 {2} {3}", title, message, options.Count(), GenerateDialogOptions(), type));
            if(result == DialogResult.Ok)
            {
                SelectedKeys = output.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return result;
        }

        public IEnumerable<string> SelectedKeys { get; set; }
        
        protected string GenerateDialogOptions(bool? forcedState = null)
        {
            var result = options.Aggregate(string.Empty, (curr, next) => string.Format("{0} {1} \"{2}\" {3}", curr, next.Item1, next.Item2, 
                forcedState.HasValue
                    ? (forcedState.Value
                        ? "on" 
                        : "off") 
                    : (SelectedKeys != null && SelectedKeys.Contains(next.Item1)) 
                        ? "on" 
                        : selectableValues 
                            ? "off"
                            : string.Empty));
            return result;
        }

        protected readonly IEnumerable<Tuple<string, string>> options;
        protected readonly string type;
        private readonly bool selectableValues;
    }
}

