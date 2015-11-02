//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Bootstrap
{
    public class InputboxDialog : Dialog
    {
        public InputboxDialog(string title, string label, string value) : base(title, value)
        {
            this.label = label;
        }
        
        public override DialogResult Show()
        {
            return Show(string.Format("--backtitle \"{0}\" --inputbox \"{1}\" 0 0 \"{2}\"", title, label, message));
        }
        
        public string Value { get { return output; } }
        
        private string label;
    }
}

