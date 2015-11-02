//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Bootstrap
{
    public class MessageDialog : Dialog
    {
        public MessageDialog(string title, string message) : base(title, message)
        {
        }

        public override DialogResult Show()
        {
            return Show(string.Format("--backtitle \"{0}\" --msgbox \"{1}\" 0 0", title, message));
        }
    }
}

