//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using Antmicro.Migrant;
using Antmicro.Migrant.Customization;
using Emul8.Peripherals;
using Emul8.Core;
using Emul8.Logging;
using System.Collections.Generic;

namespace Emul8.EventRecording
{
    public sealed class Recorder : IDisposable
    {
        public Recorder(FileStream stream, Machine machine, RecordingBehaviour recordingBehaviour)
        {
            this.stream = stream;
            this.machine = machine;
            this.recordingBehaviour = recordingBehaviour;
            nullifiedHandlersCache = new Dictionary<Delegate, Delegate>();
            openStreamSerializer = new Serializer(new Settings(useBuffering: false, disableTypeStamping: true)).ObtainOpenStreamSerializer(stream);
        }

        public void Record<T>(T value, Action<T> handler, long syncNumber, bool domainExternal)
        {
            string name;
            if(!TryExtractName(handler, out name))
            {
                return;
            }
            var recordEntry = new RecordEntry<T>(name, value, GetNullifiedHandler<Action<T>>(handler), syncNumber);
            RecordInner(recordEntry, domainExternal);
        }

        public void Record<T1, T2>(T1 value1, T2 value2, Action<T1, T2> handler, long syncNumber, bool domainExternal)
        {
            string name;
            if(!TryExtractName(handler, out name))
            {
                return;
            }
            var recordEntry = new RecordEntry<T1, T2>(name, value1, value2, GetNullifiedHandler<Action<T1, T2>>(handler), syncNumber);
            RecordInner(recordEntry, domainExternal);
        }
            
        public void Dispose()
        {
            stream.Dispose();
        }

        private bool TryExtractName(Delegate handler, out string name)
        {
            name = string.Empty;
            var peripheral = handler.Target as IPeripheral;
            if(peripheral == null || !machine.TryGetAnyName(peripheral, out name))
            {
                machine.Log(LogLevel.Warning, "Record request by a non-peripheral or not named peripheral. Ignored.");
                return false;
            }
            return true;
        }

        private void RecordInner(IRecordEntry recordEntry, bool domainExternal)
        {
            if(!domainExternal && recordingBehaviour == RecordingBehaviour.DomainExternal)
            {
                return;
            }
            openStreamSerializer.Serialize(recordEntry);
        }

        private T GetNullifiedHandler<T>(Delegate handler)
        {
            // this function should ideally have something like where T : Delegate
            // but it is not possible; this is why we cast through object
            // actually, the type expected as T are Action<?, ...>

            // we remove the target from the delegate - we don't want to serialize the peripheral
            // (it can be later get by its name), but we definitely would like to save MethodInfo
            //handler = GetNullifiedHandler(handler);
            if(nullifiedHandlersCache.ContainsKey(handler))
            {
                return (T)(object)nullifiedHandlersCache[handler];
            }
            var result = (T)(object)Delegate.CreateDelegate(typeof(T), null, handler.Method);
            nullifiedHandlersCache.Add(handler, (Delegate)(object)result);
            return result;
        }

        private readonly Serializer.OpenStreamSerializer openStreamSerializer;
        private readonly Dictionary<Delegate, Delegate> nullifiedHandlersCache;
        private readonly RecordingBehaviour recordingBehaviour;
        private readonly FileStream stream;
        private readonly Machine machine;
    }
}

