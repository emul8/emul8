//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using AntShell.Terminal;
using Emul8.Utilities;

namespace Emul8.Utilities
{
    public class SocketIOSource : IActiveIOSource
    {
        public SocketIOSource(int port)
        {
            server = new SocketServerProvider();
            server.Start(port);
        }

        public void Dispose()
        {
            server.Stop();
        }

        public void Flush()
        {
        }

        public void Write(byte b)
        {
            server.SendByte(b);
        }

        public event System.Action<byte> ByteRead
        {
            add { server.DataReceived += value; }
            remove { server.DataReceived -= value; }
        }

        private readonly SocketServerProvider server;
    }
}

