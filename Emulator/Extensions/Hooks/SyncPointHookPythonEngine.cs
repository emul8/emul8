//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Microsoft.Scripting.Hosting;
using Antmicro.Migrant.Hooks;
using Antmicro.Migrant;
using Emul8.Core;

namespace Emul8.Hooks
{
    public class SyncPointHookPythonEngine : PythonEngine
    {
        public SyncPointHookPythonEngine(string script, Emulation emulation)
        {      
            this.script = script;
            this.emulation = emulation;
            InnerInit();

            Hook = new Action<long>(syncCount =>
            {   
                Scope.SetVariable("syncCount", syncCount);
                Source.Value.Execute(Scope);
            });
        }

        [PostDeserialization]
        private void InnerInit()
        {
            Scope.SetVariable("self", emulation);
            Source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(script));
        }

        public Action<long> Hook { get; private set; }

        private readonly string script;
        private readonly Emulation emulation;

        [Transient]
        private Lazy<ScriptSource> Source;
    }
}

