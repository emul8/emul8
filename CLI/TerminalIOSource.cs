//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Terminal;
using TermSharp;
using TermSharp.Vt100;
using Emul8.Plugins.XwtProviderPlugin;

namespace Emul8.CLI
{
    internal class TerminalIOSource : IActiveIOSource, IDisposable
    {
        public TerminalIOSource(Terminal terminal)
        {
            vt100decoder = new TermSharp.Vt100.Decoder(terminal, b => HandleInput(b), new ConsoleDecoderLogger());
            utfDecoder = new ByteUtf8Decoder(vt100decoder.Feed);
        }

        public void Dispose()
        {
            HandleInput(-1);
        }

        public void Flush() 
        {
            // do nothing
        }

        public void Write(byte b)
        {
            ApplicationExtensions.InvokeInUIThread(() => utfDecoder.Feed(b));
        }

        public void HandleInput(int b)
        {
            var br = ByteRead;
            if(br != null)
            {
                br(b);
            }
        }

        public event Action<int> ByteRead;

        public bool IsAnythingAttached { get { return ByteRead != null; } }

        private readonly TermSharp.Vt100.Decoder vt100decoder;
        private readonly ByteUtf8Decoder utfDecoder;
    }
}

