//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.Bus
{
    public class BusRangeRegistration : IRegistrationPoint
    {
        public BusRangeRegistration(Range range, long offset = 0)
        {
            Range = range;
            Offset = offset;
        }

        public BusRangeRegistration(long address, long size, long offset = 0) : 
            this(new Range(address, size), offset)
        {
        }

        public virtual string PrettyString
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            if(Offset != 0)
            {
                return string.Format ("{0} with offset {1}", Range, Offset);
            }
            return string.Format("{0}", Range);
        }

        public static implicit operator BusRangeRegistration(Range range)
        {
            return new BusRangeRegistration(range);
        }

        public Range Range { get; set; }
        public long Offset { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as BusRangeRegistration;
            if(other == null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return Range == other.Range && Offset == other.Offset;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * Range.GetHashCode() + 23 * Offset.GetHashCode();
            }
        }
    }
}

