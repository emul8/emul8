//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿namespace Emul8.Bootstrap
{
    public class Infobox : Dialog
    {
        public Infobox(string message) : base("", message)
        {
            
        }
        
        public override DialogResult Show()
        {
            return Show(string.Format("--infobox \"{0}\" 5 50", message));
        }
    }
}

