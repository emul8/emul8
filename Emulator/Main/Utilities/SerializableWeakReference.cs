//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Antmicro.Migrant;
using Antmicro.Migrant.Hooks;

namespace Emul8.Utilities
{
    public sealed class SerializableWeakReference<T> where T : class
    {
        public SerializableWeakReference(T target)
        {
            reference = new WeakReference(target);
        }

        public T Target
        {
            get
            {
                return (T)reference.Target;
            }
        }

        public bool IsAlive
        {
            get
            {
                return Target == null;
            }
        }

        [PreSerialization]
        private void BeforeSerialization()
        {
            objectToSave = Target;
        }

        [PostSerialization]
        private void AfterSerialization()
        {
            objectToSave = null;
        }

        [PostDeserialization]
        private void AfterDeserialization()
        {
            reference = new WeakReference(objectToSave);
        }

        [Transient]
        private WeakReference reference;

        private T objectToSave;
    }
}

