//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.Network
{
    public class PHYRegistrationPoint : IRegistrationPoint
    {
        public PHYRegistrationPoint(uint id)
        {
            Id = id;
        }

        public string PrettyString {
            get {
                return ToString();
            }
        }
     
        public override string ToString()
        {
            return string.Format("Address: {0}", Id);
        }
        
        public uint Id {get; private set;}

        public override bool Equals(object obj)
        {
            var other = obj as PHYRegistrationPoint;
            if(other == null)
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            return Id == other.Id;
        }
        

        public override int GetHashCode()
        {
            unchecked
            {
                return Id.GetHashCode();
            }
        }
        
    }
}

