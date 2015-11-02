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
using Emul8.Peripherals.UART;
using Antmicro.Migrant;

namespace Emul8.Peripherals.SPI
{
    public class UARTToSpiConverter : NullRegistrationPointPeripheralContainer<ISPIPeripheral>, IUART
    {
        public UARTToSpiConverter(Machine machine) : base(machine)
        {
        }

        [field: Transient]
        public event Action<byte> CharReceived;

        public override void Reset()
        {
        }

        public void WriteChar(byte value)
        {
            Machine.ReportForeignEvent(value, WriteCharInner);
        }

        public Bits StopBits
        {
            get
            {
                throw new ArgumentException();
            }
        }

        public Parity ParityBit
        {
            get
            {
                throw new ArgumentException();
            }
        }

        public uint BaudRate
        {
            get
            {
                throw new ArgumentException();
            }
        }

        private void WriteCharInner(byte value)
        {
            if(RegisteredPeripheral != null)
            {
                return;
            }
            var charReceived = CharReceived;
            var read = RegisteredPeripheral.Transmit(value);
            charReceived(read);
        }
    }
}

