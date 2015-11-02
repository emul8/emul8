//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.CPU
{
    public class CPURegistrationPoint : IRegistrationPoint
    {
        public CPURegistrationPoint(int? slot = null)
        {
            Slot = slot;
        }

        public string PrettyString
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            return string.Format("Slot: {0}", Slot);
        }

        public int? Slot { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as CPURegistrationPoint;
            if(other == null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;

            return Slot == other.Slot;
        }
        

        public override int GetHashCode()
        {
            unchecked
            {
                return Slot.GetHashCode();
            }
        }
        
    }
}

