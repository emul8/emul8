//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.IO;

namespace Emul8.Bootstrap
{
    public static class Cleaner
    {
        public static void Clean(string directory)
        {
            Directory.Delete(directory, true);
        }
    }
}

