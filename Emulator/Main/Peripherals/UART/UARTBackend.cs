//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Utilities.Collections;
using AntShell.Terminal;
using System.Threading.Tasks;
using Antmicro.Migrant;

namespace Emul8.Peripherals.UART
{
    public class UARTBackend : IAnalyzableBackend<IUART>
    {
        public UARTBackend()
        {
            history = new CircularBuffer<byte>(BUFFER_SIZE);
        }

        public void Attach(IUART uart)
        {
            UART = uart;
            UART.CharReceived += b =>
            {
                lock(history)
                {
                    history.Add(b);
                }
            };
        }

        public void BindAnalyzer(IOProvider io)
        {
            this.io = io;
            io.ByteRead += b => UART.WriteChar((byte)b);

            Task.Run(() => 
            {
                RepeatHistory();

                UART.CharReceived += b =>
                {
                    lock(history)
                    {
                        io.Write(b);
                    }
                };
            });
        }

        public IUART UART { get; private set; }

        public IAnalyzable AnalyzableElement { get { return UART; } }

        public void RepeatHistory(Action beforeRepeatingHistory = null)
        {
            lock(history)
            {
                if(beforeRepeatingHistory != null)
                {
                    beforeRepeatingHistory();
                }

                foreach(var b in history)
                {
                    io.Write(b);
                }
            }
        }

        [Transient]
        private IOProvider io;
        private readonly CircularBuffer<byte> history;

        private const int BUFFER_SIZE = 100000;
    }
}

