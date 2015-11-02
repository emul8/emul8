//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using System.Text;
using System.Text.RegularExpressions;

namespace Emul8.CLI 
{
    public class StdInOutInteraction : ICommandInteraction
    {
        private bool devNullMode;
        private bool plainOutput;
        private StringBuilder buffer;

        public event Action<byte> CharReceived;

        public StdInOutInteraction(bool plainOutput = false)
        {
            this.plainOutput = plainOutput;
            buffer = new StringBuilder();
        }

        #region ICommandInteraction implementation

        public void Write(char c, ConsoleColor? color = default(ConsoleColor?))
        {
            if (plainOutput && !devNullMode && c == 27) // ESC
            {
                devNullMode = true;
            }
            else if (plainOutput && c == 13) // /r
            {
                return;
            }

            if (plainOutput && devNullMode)
            {
                buffer.Append(c);
                var regexp = new Regex(@"\x1b\[[0-9]*;[0-9]*m");
                if (regexp.IsMatch(buffer.ToString()))
                {
                    devNullMode = false;
                    buffer.Clear();
                }
            }
            else
            {
                var cr = CharReceived;
                if (cr != null) {
                    cr((byte)c);
                }
            }
        }

        public void WriteError(string error)
        {
            foreach (var l in error.ToCharArray())
            {
                Write(l);
            }
        }

        public string ReadLine()
        {
            return String.Empty;
        }

        public string CommandToExecute { get; set; }
        public bool QuitEnvironment { get; set; }

        #endregion
    }
}

