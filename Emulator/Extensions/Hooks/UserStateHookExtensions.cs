//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;

namespace Emul8.Hooks
{
    public static class UserStateHookExtensions
    {
        public static void AddUserStateHook(this Machine machine, string stateName, string pythonScript)
        {
            var engine = new UserStatePythonEngine(machine, pythonScript);
            machine.AddUserStateHook(x => x == stateName, engine.Hook);
        }
    }
}

