//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Collections.Generic;
using Emul8.Peripherals.Bus;
using System.Collections.ObjectModel;

namespace UnitTests.Mocks
{
    public class MockGPIOByNumberConnectorPeripheral : INumberedGPIOOutput, IGPIOReceiver, IBytePeripheral
    {
        public MockGPIOByNumberConnectorPeripheral(int gpios)
        {
            var innerConnections = new Dictionary<int, IGPIO>();
            for(int i = 0; i < gpios; i++)
            {
                innerConnections[i] = new GPIO();
            }
            Connections = new ReadOnlyDictionary<int, IGPIO>(innerConnections);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<int, IGPIO> Connections { get; private set; }

        public void OnGPIO(int number, bool value)
        {

        }

        public byte ReadByte(long offset)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(long offset, byte value)
        {
            throw new NotImplementedException();
        }
    }
}

