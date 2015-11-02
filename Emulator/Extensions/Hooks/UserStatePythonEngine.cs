//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Antmicro.Migrant.Hooks;
using Microsoft.Scripting.Hosting;
using Antmicro.Migrant;

namespace Emul8.Hooks
{
    public sealed class UserStatePythonEngine : PythonEngine
    {
        public UserStatePythonEngine(Machine machine, string script)
        {
            Script = script;
            Machine = machine;

            InnerInit();

            Hook = state =>
            {
                Scope.SetVariable("state", state);
                Source.Value.Execute(Scope);
            };
        }

        [PostDeserialization]
        private void InnerInit()
        {
            Scope.SetVariable("machine", Machine);
            Scope.SetVariable("self", Machine);
            Source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(Script));
        }

        public Action<string> Hook { get; private set; }

        [Transient]
        private Lazy<ScriptSource> Source;
        private readonly string Script;
        private readonly Machine Machine;
    }
}

