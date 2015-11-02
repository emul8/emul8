//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;

namespace Emul8.Core.Structure
{
    public sealed class USBRegistrationPoint : NumberRegistrationPoint<byte?>
    {
        public USBRegistrationPoint(byte? port = null) : base(port)
        {
        }

        public override string ToString()
        {
            return string.Format("Port {0}", Address);
        }
        public override bool Equals (object obj)
        {
            var other = obj as USBRegistrationPoint;
            if(other == null)
                return false;
            return base.Equals(other);
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }
    }
}

