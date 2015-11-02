//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.UART;
using Antmicro.Migrant.Hooks;
using Microsoft.Scripting.Hosting;
using Antmicro.Migrant;

namespace Emul8.Hooks
{
    public sealed class UartPythonEngine : PythonEngine
    {
        public UartPythonEngine(Machine machine, IUART uart, string script)
        {
            Script = script;
            Uart = uart;
            Machine = machine;

            InnerInit();

            Hook = line =>
            {
                Scope.SetVariable("line", line);
                Source.Value.Execute(Scope);
            };
        }

        [PostDeserialization]
        private void InnerInit()
        {
            Scope.SetVariable("machine", Machine);
            Scope.SetVariable("uart", Uart);
            Scope.SetVariable("self", Uart);
            Source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(Script));
        }

        public Action<string> Hook { get; private set; }

        [Transient]
        private Lazy<ScriptSource> Source;
        private readonly string Script;
        private readonly IUART Uart;
        private readonly Machine Machine;
    }
}

