//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Net.Sockets;
using Emul8.Logging;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace Emul8.Utilities
{
    public class SocketServerProvider : IDisposable
    {
        public SocketServerProvider()
        {
            queue = new BlockingCollection<byte>();
            queueCancellationToken = new CancellationTokenSource();        
        }

        public void Start(int port)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(1);

            listenerThread = new Thread(ListenerThreadBody) 
            {
                IsBackground = true,
                Name = GetType().Name
            };
            listenerThread.Start();
        }

        public void Stop()
        {
            if(socket != null)
            {
                socket.Close();
            }
            queueCancellationToken.Cancel();
            listenerThreadStopped = true;
            server.Dispose();
            listenerThread.Join();
        }

        public void Dispose()
        {
            Stop();
        }

        public void SendByte(byte b)
        {
            queue.Add(b);
        }

        public event Action<Stream> ConnectionAccepted;
        public event Action<byte> DataReceived;

        private void WriterThreadBody(Stream stream)
        {
            while(true)
            {
                try
                {
                    stream.WriteByte(queue.Take(queueCancellationToken.Token));
                }
                catch(OperationCanceledException)
                {
                    break;
                }
                catch(IOException)
                {
                    break;
                }
                catch(ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private void ReaderThreadBody(Stream stream)
        {
            while(true)
            {
                int value;
                try
                {
                    value = stream.ReadByte();
                }
                catch(IOException)
                {
                    value = -1;
                }

                if(value == -1)
                {
                    Logger.LogAs(this, LogLevel.Debug, "Client disconnected, stream closed.");
                    break;
                }

                var dataReceived = DataReceived;
                if(dataReceived != null)
                {
                    dataReceived((byte)value);
                }
            }
        }

        private void ListenerThreadBody()
        {
            NetworkStream stream;
            listenerThreadStopped = false;
            while(!listenerThreadStopped)
            {
                try
                {
                    socket = server.Accept();
                    stream = new NetworkStream(socket);
                }
                catch(SocketException)
                {
                    break;
                }

                var connectionAccepted = ConnectionAccepted;
                if(connectionAccepted != null)
                {
                    connectionAccepted(stream);
                }

                var writerThread = new Thread(() => WriterThreadBody(stream)) {
                    Name = GetType().Name + "_WriterThread",
                    IsBackground = true
                };

                var readerThread = new Thread(() => ReaderThreadBody(stream)) {
                    Name = GetType().Name + "_ReaderThread",
                    IsBackground = true
                };

                writerThread.Start();
                readerThread.Start();

                writerThread.Join();
                readerThread.Join();
            }
        }

        private readonly CancellationTokenSource queueCancellationToken;
        private readonly BlockingCollection<byte> queue;

        private bool listenerThreadStopped;
        private Thread listenerThread;
        private Socket server;
        private Socket socket;
    }
}
