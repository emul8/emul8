//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Core
{
    public class Platform
    {
        public Platform()
        {
            HasMemory = true;
        }       

        public override string ToString()
        {
            return Name;
        }

        public string Name        { get; set; }
        public string ScriptPath  { get; set; }
        public bool   HasFlash    { get; set; }
        public bool   HasPendrive { get; set; }
        public bool   HasMemory   { get; set; }
        public string IconResource { get; set; }
    }
}

