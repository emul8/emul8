//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using Emul8.Backends.Terminals;
using Emul8.Logging;

namespace Emul8.Peripherals.SPI
{
    public class FakeEfmSPITransmitter : BackendTerminal
    {
        public FakeEfmSPITransmitter()
        {
            responses = new Dictionary<int, byte>();
        }

        public void AddResponse(int byteNumber, byte data)
        {
            responses.Add(byteNumber, data);
        }

        public override void WriteChar(byte data)
        {
            // write the response char
            responses.TryGetValue(currentByteNo, out data);
            CallCharReceived(data);
            this.Log(LogLevel.Info, "Sent 0x{0:X} in {1} turn.", data, currentByteNo);
            currentByteNo++;
        }

        private int currentByteNo;
        private readonly Dictionary<int, byte> responses;
    }
}

