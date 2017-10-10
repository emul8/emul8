//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Core;
using Antmicro.Migrant;

namespace Emul8.Peripherals.UART
{
    public abstract class UARTBase : IUART
    {
        protected UARTBase(Machine machine)
        {
            queue = new Queue<byte>();
            Machine = machine;
        }

        public void WriteChar(byte value)
        {
            Machine.ReportForeignEvent(value, WriteCharInner);
        }

        public virtual void Reset()
        {
            ClearBuffer();
        }

        [field: Transient]
        public event Action<byte> CharReceived;

        protected abstract void CharWritten();
        protected abstract void QueueEmptied();

        protected bool TryGetCharacter(out byte character)
        {
            lock(queue)
            {
                if(queue.Count == 0)
                {
                    character = default(byte);
                    return false;
                }
                character = queue.Dequeue();
                if(queue.Count == 0)
                {
                    QueueEmptied();
                }
                return true;
            }
        }

        protected void TransmitCharacter(byte character)
        {
            CharReceived?.Invoke(character);
        }

        protected void ClearBuffer()
        {
            lock(queue)
            {
                queue.Clear();
                QueueEmptied();
            }
        }

        protected int Count
        {
            get
            {
                lock(queue)
                {
                    return queue.Count;
                }
            }
        }

        private void WriteCharInner(byte value)
        {
            lock(queue)
            {
                queue.Enqueue(value);
                CharWritten();
            }
        }

        protected readonly Machine Machine;
        private readonly Queue<byte> queue;

        public abstract Bits StopBits { get; }

        public abstract Parity ParityBit { get; }

        public abstract uint BaudRate { get; }
    }
}

