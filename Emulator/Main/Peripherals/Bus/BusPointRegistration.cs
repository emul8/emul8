//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.Bus
{
    public class BusPointRegistration : IRegistrationPoint
    {
        public BusPointRegistration(long address, long offset = 0)
        {
            StartingPoint = address;
            Offset = offset;
        }
        
        public override string ToString()
        {
            return string.Format ("{0} with offset {1}", StartingPoint, Offset);
        }

        public string PrettyString
        {
            get
            {
                return ToString();
            }
        }

        public static implicit operator BusPointRegistration(long address)
        {
            return new BusPointRegistration(address);
        }
        
        public long StartingPoint { get; set; }
        public long Offset { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as BusPointRegistration;
            if(other == null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;

            return StartingPoint == other.StartingPoint && Offset == other.Offset;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * StartingPoint.GetHashCode() + 23 * Offset.GetHashCode();
            }
        }
    }
}

