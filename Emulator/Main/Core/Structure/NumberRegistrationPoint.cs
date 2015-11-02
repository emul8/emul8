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
    //TODO: constraint on T
    public class NumberRegistrationPoint<T> : IRegistrationPoint
    {
        public T Address { get; private set ;}
        
        public NumberRegistrationPoint(T address)
        {
            Address = address;
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
            return string.Format("Address: {0}", Address);
        }

        public override bool Equals(object obj)
        {
            var other = obj as NumberRegistrationPoint<T>;
            if(other == null)
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            return Address.Equals(other.Address);
        }
        

        public override int GetHashCode()
        {
            unchecked
            {
                return (Address != null ? Address.GetHashCode() : 0);
            }
        }
        
    }
}

