//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core.Structure
{
    public sealed class AttachedRegistrationPoint : ITheOnlyPossibleRegistrationPoint
    {
        public static AttachedRegistrationPoint Instance { get; private set; }

        static AttachedRegistrationPoint()
        {
            Instance = new AttachedRegistrationPoint();
        }

        public string PrettyString
        {
            get
            {
                return "attached";
            }
        }

        public override string ToString()
        {
            return PrettyString;
        }

        private AttachedRegistrationPoint()
        {
        }
    }
}

