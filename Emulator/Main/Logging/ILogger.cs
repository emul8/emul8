//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Logging
{
    public interface ILogger : IDisposable
    {
        string GetMachineName(int id);
        string GetObjectName(int id);

        int GetOrCreateSourceId(object source);
        bool TryGetName(int id, out string objectName, out string machineName);
        bool TryGetSourceId(object source, out int id);
    }
}

