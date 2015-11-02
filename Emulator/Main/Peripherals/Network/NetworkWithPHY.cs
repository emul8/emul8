//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Core.Structure;
using Emul8.Exceptions;

namespace Emul8.Peripherals.Network
{
    public abstract class NetworkWithPHY: IPeripheral, IPeripheralContainer<IPhysicalLayer<ushort>, PHYRegistrationPoint>
    {
        protected NetworkWithPHY(Machine machine)
        {
            phys = new Dictionary<uint, IPhysicalLayer<ushort>>();
            this.machine = machine;
        }

        #region IRegister[IPHYInterface,PHYRegistrationPoint] implementation
        public void Register(IPhysicalLayer<ushort> peripheral, PHYRegistrationPoint registrationPoint)
        {
            if (phys.ContainsKey(registrationPoint.Id))
            {
                throw new RecoverableException("Selected registration port is already taken.");
            }
            phys.Add(registrationPoint.Id, peripheral);
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Unregister(IPhysicalLayer<ushort> peripheral)
        {
            var key = phys.First(x => x.Value == peripheral).Key;
            phys.Remove(key);
            machine.UnregisterAsAChildOf(this, peripheral);
        }
        #endregion


        #region IContainer[IPHYInterface,PHYRegistrationPoint] implementation
        public IEnumerable<PHYRegistrationPoint> GetRegistrationPoints (IPhysicalLayer<ushort> peripheral)
        {
            return phys.Select(x => new PHYRegistrationPoint(x.Key));
        }

        public IEnumerable<IRegistered<IPhysicalLayer<ushort>, PHYRegistrationPoint>> Children 
        {
            get 
            {
                return phys.Select(x => Registered.Create(x.Value, new PHYRegistrationPoint(x.Key)));
            }
        }
        #endregion
        
        protected Dictionary<uint, IPhysicalLayer<ushort>> phys;
        protected Machine machine;

        public abstract void Reset();
        
    }
}

