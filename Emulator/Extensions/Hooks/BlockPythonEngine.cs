//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.CPU;
using Microsoft.Scripting.Hosting;
using Antmicro.Migrant.Hooks;
using Antmicro.Migrant;

namespace Emul8.Hooks
{
    public sealed class BlockPythonEngine : PythonEngine
    {
        public BlockPythonEngine(Machine mach, ICPUWithHooks cpu, string script)
        {
            Script = script;
            CPU = cpu;
            Machine = mach;

            InnerInit();

            Hook = (pc) =>
            {
                Scope.SetVariable("pc", pc);
                Source.Value.Execute(Scope);
            };

            HookWithSize = (pc, size) =>
            {
                Scope.SetVariable("pc", pc);
                Scope.SetVariable("size", size);
                Source.Value.Execute(Scope);
            };
        }

        [PostDeserialization]
        private void InnerInit()
        {
            Scope.SetVariable("machine", Machine);
            Scope.SetVariable("cpu", CPU);
            Scope.SetVariable("self", CPU);
            Source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(Script));
        }

        public Action<uint> Hook { get; private set; }

        public Action<uint, uint> HookWithSize { get; private set; }

        [Transient]
        private Lazy<ScriptSource> Source;
        private readonly string Script;
        private readonly ICPUWithHooks CPU;
        private readonly Machine Machine;
    }
}

