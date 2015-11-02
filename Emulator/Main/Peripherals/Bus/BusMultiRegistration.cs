//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Exceptions;

namespace Emul8.Peripherals.Bus
{
    public class BusMultiRegistration : BusRangeRegistration
    {
        public BusMultiRegistration(long address, long size, string region) : base(address, size)
        {
            if(string.IsNullOrWhiteSpace(region))
            {
                throw new ConstructionException("'Region' parameter cannot be null or empty.");
            }
            Address = address;
            ConnectionRegionName = region;
        }

        public long Address { get; private set; }
        public string ConnectionRegionName { get; private set; }
        public override string PrettyString { get { return ToString(); } }

        public override bool Equals(object obj)
        {
            var other = obj as BusMultiRegistration;
            if(other == null)
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            return Address == other.Address && ConnectionRegionName == other.ConnectionRegionName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 * Address.GetHashCode() + 101 * ConnectionRegionName.GetHashCode();
            }
        }        
    }
}

