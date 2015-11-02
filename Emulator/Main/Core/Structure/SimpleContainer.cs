//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;
using System.Collections.Generic;
using Emul8.Exceptions;
using System.Linq;

namespace Emul8.Core.Structure
{
    public abstract class SimpleContainer<T> : IPeripheralContainer<T, NumberRegistrationPoint<int>>, IPeripheral
         where T : IPeripheral
    {

        public virtual IEnumerable<NumberRegistrationPoint<int>> GetRegistrationPoints(T peripheral)
        {
            return ChildCollection.Keys.Select(x => new NumberRegistrationPoint<int>(x)).ToList();
        }

        public virtual IEnumerable<IRegistered<T, NumberRegistrationPoint<int>>> Children
        {
            get
            {
                return ChildCollection.Select(x => Registered.Create(x.Value, new NumberRegistrationPoint<int>(x.Key))).ToList();
            }
        }

        public abstract void Reset();

        public virtual void Register(T peripheral, NumberRegistrationPoint<int> registrationPoint)
        {
            if(ChildCollection.ContainsKey(registrationPoint.Address))
            {
                throw new RegistrationException("The specified registration point is already in use.");
            }
            ChildCollection.Add(registrationPoint.Address, peripheral);
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public virtual void Unregister(T peripheral)
        {
            var toRemove = ChildCollection.Where(x => x.Value.Equals(peripheral)).Select(x => x.Key).ToList();
            if(toRemove.Count == 0)
            {
                throw new RegistrationException("The specified peripheral was never registered.");
            }
            foreach(var key in toRemove)
            {
                ChildCollection.Remove(key);
            }
            machine.UnregisterAsAChildOf(this, peripheral);
        }

        protected T GetByAddress(int address)
        {
            T peripheral;
            if(!TryGetByAddress(address, out peripheral))
            {
                throw new KeyNotFoundException();
            }
            return peripheral;
        }

        protected bool TryGetByAddress(int address, out T peripheral)
        {
            return ChildCollection.TryGetValue(address, out peripheral);
        }

        protected SimpleContainer(Machine machine)
        {
            this.machine = machine;
            ChildCollection =  new Dictionary<int, T>();
        }

        protected Dictionary<int, T> ChildCollection;
        protected readonly Machine machine;
    }
}

