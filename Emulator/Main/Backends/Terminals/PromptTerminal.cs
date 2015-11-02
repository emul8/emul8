//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Antmicro.Migrant;

namespace Emul8.Backends.Terminals
{
    [Transient]
    public sealed class PromptTerminal : BackendTerminal
    {
        public PromptTerminal(Action<string> onLine = null, Action onPrompt = null, string prompt = "@/ # ")
        {
            if(prompt.Length == 0)
            {
                throw new ArgumentException("Prompt cannot be empty.");
            }
            charBuffer = new List<char>();
            SetPrompt(prompt);
            this.onLine = onLine;
            this.onPrompt = onPrompt;
        }

        public override void WriteChar(byte value)
        {
            // TODO: support for different line-end marks
            if(value == 13)
            {
                return;
            }
            if(value != 10)
            {
                charBuffer.Add((char)value);
                if(index < promptBytes.Length)
                {
                    if(promptBytes[index] != value)
                    {
                        index = promptBytes.Length + 1; // this way it will never match on this line
                    }
                    if(index == promptBytes.Length - 1 && onPrompt != null)
                    {
                        onPrompt();
                    }
                    index++;
                }
                return;
            }
            // TODO: little optimization here
            if(onLine != null)
            {
                onLine(new String(charBuffer.ToArray()));
            }
            charBuffer.Clear();
            index = 0;
        }

        public string GetWaitingLine()
        {
            return new string(charBuffer.ToArray());
        }

        public void WriteStringToTerminal(string line)
        {
            foreach(var chr in line)
            {
                CallCharReceived((byte)chr);
                WaitBeforeNextChar();
            }
        }

        public void WriteLineToTerminal(string line)
        {
            WriteStringToTerminal(line);
            CallCharReceived(10);
            WaitBeforeNextChar(); // for consistency
        }

        public void SetPrompt(string prompt)
        {
            promptBytes = prompt.ToCharArray().Select(x => (byte)x).ToArray();
        }

        public TimeSpan WriteCharDelay { get; set; }

        private void WaitBeforeNextChar()
        {
            if(WriteCharDelay != TimeSpan.Zero)
            {
                Thread.Sleep(WriteCharDelay);
            }
        }

        private readonly List<char> charBuffer;
        private readonly Action<string> onLine;
        private readonly Action onPrompt;
        private byte[] promptBytes;
        private int index;
    }
}
