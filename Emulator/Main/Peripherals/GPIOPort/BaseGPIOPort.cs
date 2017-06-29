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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Emul8.UserInterface;

namespace Emul8.Peripherals.GPIOPort
{
    [Icon("gpio")]
    public abstract class BaseGPIOPort : INumberedGPIOOutput, IPeripheralRegister<IGPIOReceiver, NullRegistrationPoint>,
        IPeripheralRegister<IGPIOSender, NullRegistrationPoint>, IPeripheralRegister<IGPIOReceiver, NumberRegistrationPoint<int>>,
        IPeripheral, IGPIOReceiver, IPeripheralRegister<IGPIOSender, NumberRegistrationPoint<int>>
    {
        public void Register(IGPIOSender peripheral, NumberRegistrationPoint<int> registrationPoint)
        {
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Unregister(IGPIOSender peripheral)
        {
            machine.UnregisterAsAChildOf(this, peripheral);
        }

        public void Register(IGPIOSender peripheral, NullRegistrationPoint registrationPoint)
        {
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Register(IGPIOReceiver peripheral, NullRegistrationPoint registrationPoint)
        {
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public void Unregister(IGPIOReceiver peripheral)
        {
            machine.UnregisterAsAChildOf(this, peripheral);
            foreach(var gpio in Connections.Values)
            {
                var endpoint = gpio.Endpoint;
                if(endpoint != null && endpoint.Number == 0 && endpoint.Receiver == peripheral)
                {
                    gpio.Disconnect();
                }
            }
        }

        public void Register(IGPIOReceiver peripheral, NumberRegistrationPoint<int> registrationPoint)
        {          
            machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
        }

        public virtual void Reset()
        {
            foreach(var connection in Connections.Values)
            {
                connection.Unset();
            }
            for(int i = 0; i < State.Length; ++i)
            {
                State[i] = false;
            }
        }

        protected BaseGPIOPort(Machine machine, int numberOfConnections)
        {
            var innerConnections = new Dictionary<int, IGPIO>();
            State = new bool[numberOfConnections];
            for(var i = 0; i < numberOfConnections; i++)
            {
                innerConnections[i] = new GPIO();
            }
            for(var i = 1; i < numberOfConnections; i++)
            {
                innerConnections[-i] = new GPIO();
            }
            this.machine = machine;
            Connections = new ReadOnlyDictionary<int, IGPIO>(innerConnections);
        }

        protected void SetConnectionsStateUsingBits(uint bits)
        {
            foreach(var cn in Connections)
            {
                if((bits & 1u << cn.Key) != 0)
                {
                    cn.Value.Set();
                }
                else
                {
                    cn.Value.Unset();
                }
            }
        }

        public virtual void OnGPIO(int number, bool value)
        {
            //GPIOs from outer peripherals have to be attached by their negative value.
            //Please keep in mind that it's impossible to connect outgoing GPIO to pin 0.
            State[number] = value;
        }

        public IReadOnlyDictionary<int, IGPIO> Connections { get; private set; }

        protected bool[] State;
        private readonly Machine machine;
    }
}

