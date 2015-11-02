//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace Emul8.Bootstrap
{
    public class RadiolistDialog : ListDialog
    {
        public RadiolistDialog(string title, string message, IEnumerable<Tuple<string, string>> options) : base("radiolist", title, message, true, options)
        {
        }
        
        public override DialogResult Show()
        {
            if(SelectedKeys == null || !SelectedKeys.Any())
            {
                SelectedKeys = new [] { options.First().Item1 };
            }
            return base.Show();
        }
    }
}

