//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Bus;
using Emul8.Logging;
using Emul8.Core;
using System.Collections.Generic;
using Emul8.Core.Structure.Registers;

namespace Emul8.Peripherals.UART
{
    public sealed class Stm32F7Usart : IDoubleWordPeripheral, IKnownSize, IUART
    {
        public Stm32F7Usart(Machine machine)
        {
            this.machine = machine;
            IRQ = new GPIO();
            receiveQueue = new Queue<byte>();

            registers = new DoubleWordRegisterCollection(this, new Dictionary<long, DoubleWordRegister> {
                { (long)Register.InterruptAndStatus, new DoubleWordRegister(this, 0x200000C0).
                    WithFlag(6, FieldMode.Read, name: "TC").WithFlag(7, FieldMode.Read, name: "TXE").WithValueField(22, 9, FieldMode.Read, name: "Reserved") }
            });
        }

        public uint ReadDoubleWord(long offset)
        {
            switch((Register)offset)
            {
            case Register.InterruptAndStatus:
                return 0x200000C0;
            case Register.ReceiveData:
                return receiveQueue.Dequeue();
            default:
                return registers.Read(offset);
            }
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            switch((Register)offset)
            {
            case Register.TransmitData:
                var charReceived = CharReceived;
                charReceived((byte)value);
                break;
            default:
                registers.Write(offset, value);
                break;
            }
        }

        public void Reset()
        {
            
        }

        public void WriteChar(byte value)
        {
            machine.ReportForeignEvent(value, WriteCharInner);
        }

        public uint BaudRate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Bits StopBits
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Parity ParityBit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event Action<byte> CharReceived;

        public GPIO IRQ { get; private set; }

        public long Size
        {
            get
            {
                return 0x400;
            }
        }

        private void WriteCharInner(byte value)
        {
            receiveQueue.Enqueue(value);
        }

        private readonly Queue<byte> receiveQueue;
        private readonly Machine machine;
        private readonly DoubleWordRegisterCollection registers;

        private enum Register
        {
            InterruptAndStatus = 0x1C,
            ReceiveData = 0x24,
            TransmitData = 0x28
        }
    }
}

