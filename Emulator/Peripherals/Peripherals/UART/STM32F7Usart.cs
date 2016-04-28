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
            sync = new object();
            this.machine = machine;
            IRQ = new GPIO();
            receiveQueue = new Queue<byte>();

            var controlRegister1 = new DoubleWordRegister(this);
            enabled = controlRegister1.DefineFlagField(0);
            receiveEnabled = controlRegister1.DefineFlagField(2);
            transmitEnabled = controlRegister1.DefineFlagField(3);
            receiveInterruptEnabled = controlRegister1.DefineFlagField(5);

            registers = new DoubleWordRegisterCollection(this, new Dictionary<long, DoubleWordRegister> {
                { (long)Register.ControlRegister1, controlRegister1 },
                { (long)Register.ControlRegister2, new DoubleWordRegister(this) },
                { (long)Register.ControlRegister3, new DoubleWordRegister(this).WithFlag(0) },
                { (long)Register.InterruptAndStatus, new DoubleWordRegister(this, 0x200000C0)
                        .WithFlag(5, FieldMode.Read, name: "RXNE", valueProviderCallback: delegate { return receiveQueue.Count > 0; })
                        .WithFlag(6, FieldMode.Read, name: "TC").WithFlag(7, FieldMode.Read, name: "TXE")
                        .WithValueField(22, 9, FieldMode.Read, name: "Reserved") }
            });
            registers.Reset();
        }

        public uint ReadDoubleWord(long offset)
        {
            lock(sync)
            {
                switch((Register)offset)
                {
                case Register.BaudRate:
                    return baudRateDivisor;
                case Register.ReceiveData:
                    return HandleReceiveData();
                default:
                    return registers.Read(offset);
                }
            }
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            lock(sync)
            {
                switch((Register)offset)
                {
                case Register.BaudRate:
                    baudRateDivisor = value;
                    break;
                case Register.TransmitData:
                    HandleTransmitData(value);
                    break;
                default:
                    if(offset != 0x1C)
                    {
                        registers.Write(offset, value);
                    }
                    break;
                }
            }
        }

        public void Reset()
        {
            registers.Reset();
        }

        public void WriteChar(byte value)
        {
            machine.ReportForeignEvent(value, WriteCharInner);
        }

        public uint BaudRate
        {
            get
            {
                return ApbClock / baudRateDivisor;
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
            lock(sync)
            {
                if(receiveEnabled.Value && enabled.Value)
                {
                    receiveQueue.Enqueue(value);
                    if(receiveInterruptEnabled.Value)
                    {
                        IRQ.Set();
                    }
                }
                else
                {
                    this.Log(LogLevel.Warning, "Char was received, but the receiver (or the whole USART) is not enabled. Ignoring.");
                }
            }
        }

        private void HandleTransmitData(uint value)
        {
            if(transmitEnabled.Value && enabled.Value)
            {
                var charReceived = CharReceived;
                charReceived((byte)value);
            }
            else
            {
                this.Log(LogLevel.Warning, "Char was to be sent, but the transmitter (or the whole USART) is not enabled. Ignoring.");
            }
        }

        private uint HandleReceiveData()
        {
            var result = receiveQueue.Dequeue();
            if(receiveInterruptEnabled.Value)
            {
                IRQ.Set(receiveQueue.Count > 0);
            }
            return result;
        }

        private uint baudRateDivisor;
        private readonly IFlagRegisterField receiveInterruptEnabled;
        private readonly IFlagRegisterField transmitEnabled;
        private readonly IFlagRegisterField receiveEnabled;
        private readonly IFlagRegisterField enabled;
        private readonly Queue<byte> receiveQueue;
        private readonly Machine machine;
        private readonly DoubleWordRegisterCollection registers;
        private readonly object sync;

        private const uint ApbClock = 200000000;

        private enum Register
        {
            ControlRegister1 = 0x0,
            ControlRegister2 = 0x4,
            ControlRegister3 = 0x8,
            BaudRate = 0xC,
            InterruptAndStatus = 0x1C,
            ReceiveData = 0x24,
            TransmitData = 0x28
        }
    }
}

