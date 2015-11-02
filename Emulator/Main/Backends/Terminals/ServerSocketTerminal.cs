//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using Emul8.Core;
using Emul8.Utilities;

namespace Emul8.Backends.Terminals
{
    public static class ServerSocketTerminalExtensions
    {
        public static void CreateServerSocketTerminal(this Emulation emulation, int port, string name, bool emitConfig = false)
        {
            emulation.ExternalsManager.AddExternal(new ServerSocketTerminal(port, emitConfig), name);
        }
    }

    public class ServerSocketTerminal : BackendTerminal, IDisposable
    {
        public ServerSocketTerminal(int port, bool emitConfigBytes = true)
        {
            server = new SocketServerProvider();
            server.DataReceived += CallCharReceived;
            server.ConnectionAccepted += s =>
            {
                if(!emitConfigBytes)
                {
                    return;
                }

                var initBytes = new byte[] { 
                    255, 253, 000, // IAC DO    BINARY
                    255, 251, 001, // IAC WILL  ECHO
                    255, 251, 003, // IAC WILL  SUPPRESS_GO_AHEAD
                    255, 252, 034, // IAC WONT  LINEMODE
                };
                s.Write(initBytes, 0, initBytes.Length);
                try
                {
                    // we expect 9 bytes as a result of sending
                    // config bytes
                    for(int i = 0; i < 9; i++)
                    {
                        s.ReadByte();
                    }
                }
                catch(ObjectDisposedException)
                {
                    // intentionally left blank
                }
            };

            server.Start(port);
        }

        public override void WriteChar(byte value)
        {
            server.SendByte(value);
        }

        public void Dispose()
        {
            server.Stop();
        }

        private readonly SocketServerProvider server;
    }
}

