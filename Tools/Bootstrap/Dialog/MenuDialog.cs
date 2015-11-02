//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Collections.Generic;
using System;

namespace Emul8.Bootstrap
{
    public class MenuDialog : ListDialog
    {
        public MenuDialog(string title, string message, IEnumerable<Tuple<string, string>> options) : base("menu", title, message, false, options)
        {
        }
    }
}

