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
    public sealed class NullRegistrationPoint : ITheOnlyPossibleRegistrationPoint
    {
        public static NullRegistrationPoint Instance { get; private set; }

        public string PrettyString
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            return "[-]";
        }

        static NullRegistrationPoint()
        {
            Instance = new NullRegistrationPoint();
        }

        private NullRegistrationPoint()
        {
        }
    }
}

