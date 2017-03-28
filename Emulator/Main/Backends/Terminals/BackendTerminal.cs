//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.UART;
using Emul8.Core;

namespace Emul8.Backends.Terminals
{
    public abstract class BackendTerminal : IExternal, IConnectable<IUART>
    {
        public virtual event Action<byte> CharReceived;

        public abstract void WriteChar(byte value);

        public virtual void AttachTo(IUART uart)
        {
            CharReceived += uart.WriteChar;
            uart.CharReceived += WriteChar;
        }
        public virtual void DetachFrom(IUART uart)
        {
            CharReceived -= uart.WriteChar;
            uart.CharReceived -= WriteChar;
        }

        protected void CallCharReceived(byte value)
        {
            var charReceived = CharReceived;
            if(charReceived != null)
            {
                charReceived(value);
            }
        }
    }
}

