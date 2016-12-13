//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.EventRecording
{
    // this is struct to prevent open stream serializer from caching all classes
    internal struct RecordEntryBase
    {
        public RecordEntryBase(string name, Delegate handler, long syncNumber) : this()
        {
            this.Name = name;
            this.SyncNumber = syncNumber;
            if(handler.Target != null)
            {
                throw new ArgumentException("Assertion failed: the handler is supposed to have null target.");
            }
            this.Handler = handler;
        }

        public override string ToString()
        {
            return string.Format("[RecordEntry: Handler={0}, Name={1}, SyncNumber={2}]", Handler.Method.Name, Name, SyncNumber);
        }

        public string Name { get; private set; }
        public long SyncNumber { get; private set; }

        public readonly Delegate Handler;
    }
}

