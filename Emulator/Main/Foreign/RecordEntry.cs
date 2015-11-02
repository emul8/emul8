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
    internal class RecordEntry<T> : IRecordEntry
    {  
        public RecordEntry(string name, T value, Action<T> handler, long syncNumber)
        {
            Value = value;
            @base = new RecordEntryBase(name, handler, syncNumber);
        }

        public void Play(Func<string, Delegate, Delegate> handlerResolver)
        {
            ((Action<T>)handlerResolver(Name, @base.Handler))(Value);
        }

        public override string ToString()
        {
            return string.Format("[RecordEntry: Base={0}, Value={1}]", @base, Value);
        }

        public T Value { get; private set; }

        public string Name
        {
            get
            {
                return @base.Name;
            }
        }

        public long SyncNumber
        {
            get
            {
                return @base.SyncNumber;
            }
        }

        private RecordEntryBase @base;
    }

    internal class RecordEntry<T1, T2> : IRecordEntry
    {
        public RecordEntry(string name, T1 value1, T2 value2, Action<T1, T2> handler, long syncNumber)
        {
            Value1 = value1;
            Value2 = value2;
            @base = new RecordEntryBase(name, handler, syncNumber);
        }

        public void Play(Func<string, Delegate, Delegate> handlerResolver)
        {
            ((Action<T1, T2>)handlerResolver(Name, @base.Handler))(Value1, Value2);
        }

        public override string ToString()
        {
            return string.Format("[RecordEntry: Base={0}, Value1={1}, Value2={2}]", @base, Value1, Value2);
        }

        public T1 Value1 { get; private set; }
        public T2 Value2 { get; private set; }

        public string Name
        {
            get
            {
                return @base.Name;
            }
        }

        public long SyncNumber
        {
            get
            {
                return @base.SyncNumber;
            }
        }

        private RecordEntryBase @base;
    }
}

