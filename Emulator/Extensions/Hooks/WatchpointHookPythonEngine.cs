//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Antmicro.Migrant.Hooks;
using Microsoft.Scripting.Hosting;
using Antmicro.Migrant;
using Emul8.Peripherals.Bus;

namespace Emul8.Hooks
{
    public class WatchpointHookPythonEngine : PythonEngine
    {
        public WatchpointHookPythonEngine(SystemBus sysbus, string script)
        {
            this.sysbus = sysbus;
            this.script = script;

            InnerInit();
            Hook = (address, width) =>
            {
                Scope.SetVariable("address", address);
                Scope.SetVariable("width", width);
                source.Value.Execute(Scope);
            };
        }

        public Action<long, Width> Hook { get; private set; }

        [PostDeserialization]
        private void InnerInit()
        {
            Scope.SetVariable("self", sysbus);

            source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(script));
        }

        [Transient]
        private Lazy<ScriptSource> source;

        private readonly string script;
        private readonly object sysbus;
    }
}

