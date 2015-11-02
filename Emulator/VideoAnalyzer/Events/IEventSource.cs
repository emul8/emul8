//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Extensions.Analyzers.Video.Handlers;

namespace Emul8.Extensions.Analyzers.Video.Events
{
    internal interface IEventSource
    {
        void AttachHandler(IOHandler h);
        void DetachHandler();

        int X { get; }
        int Y { get; }
    }
}

