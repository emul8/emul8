//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.UART;
using Emul8.Utilities;
using Emul8.Core;

namespace Emul8.Hooks
{
    public static class UartHooksExtensions
    {
        public static void AddCharHook(this IUART uart, Func<byte, bool> predicate, Action<byte> hook)
        {
            uart.CharReceived += x =>
            {
                if(predicate(x))
                {
                    hook(x);
                }
            };
        }

        public static void AddLineHook(this IUART uart, Func<string, bool> predicate, Action<string> hook)
        {
            var currentLine = string.Empty;
            uart.CharReceived += x =>
            {
                if((x == 10 || x == 13))
                {
                    if(predicate(currentLine))
                    {
                        hook(currentLine);
                    }
                    currentLine = string.Empty;
                    return;
                }
                currentLine += (char)x;
            };
        }

        public static void AddLineHook(this IUART uart, [AutoParameter] Machine machine, string contains, string pythonScript)
        {
            var engine = new UartPythonEngine(machine, uart, pythonScript);
            uart.AddLineHook(x => x.Contains(contains), engine.Hook);
        }
    }
}

