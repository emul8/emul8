//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.UART;
using NUnit.Framework;
using Emul8.Core;

namespace PeripheralsTests
{
	[TestFixture]
    public class GaislerUARTTests
	{
		[Test]		
        public void ShouldReset()
		{
            var uart = new GaislerUART(new Machine());

            uart.CharReceived += SendByte;

            // Update control and scalar register, write to buffer, check status register and then reset and verify.
            uart.WriteDoubleWord(0x08, 0xa4a5); // EC, bit 8, set writes asserts
            Assert.AreEqual(0xa4a5 & 0x7fff, uart.ReadDoubleWord(0x08));
            uart.WriteDoubleWord(0x0C, 0xb5b5);
            Assert.AreEqual(0xb5b5, uart.ReadDoubleWord(0x0C));
            // test transmit buffer
            uart.WriteDoubleWord(0x00, 0xc5);

            // test receive buffer
            uart.WriteChar(0xd5);
            Assert.AreEqual(0x04000007, uart.ReadDoubleWord(0x04));
            uart.Reset();
            Assert.AreEqual(0x06, uart.ReadDoubleWord(0x04));
            Assert.AreEqual((0xa5a5 & 0x7ebc), uart.ReadDoubleWord(0x08));
            Assert.AreEqual(0x0, uart.ReadDoubleWord(0x0C));
            Assert.AreEqual(0x0, uart.ReadDoubleWord(0x00));
		}

        private void SendByte(byte b)
        {
            Assert.AreEqual(0xc5, b);
        }
	}
}

