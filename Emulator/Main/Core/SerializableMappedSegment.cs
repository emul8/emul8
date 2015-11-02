//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Emul8.Core;
using Antmicro.Migrant.Hooks;
using Antmicro.Migrant;

namespace Emul8.Core
{
    public sealed class SerializableMappedSegment : IMappedSegment, IDisposable
    {
        public SerializableMappedSegment(int size, int startingOffset)
        {
            this.size = size;
            offset = startingOffset;
            MakeSegment();
        }

        public IntPtr Pointer
        {
            get
            {
                return pointer;
            }
        }

        public long StartingOffset
        {
            get
            {
                return offset;
            }
        }

        public long Size
        {
            get
            {
                return size;
            }
        }

        public void Touch()
        {
            if(pointer != IntPtr.Zero)
            {
                return;
            }
            pointer = Marshal.AllocHGlobal(size);
            var zeroBuf = new byte[size];
            Marshal.Copy(zeroBuf, 0, pointer, size);
        }

        public void Dispose()
        {
            var oldPointer = Interlocked.Exchange(ref pointer, IntPtr.Zero);
            if(oldPointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Pointer);
            }
        }

        [PreSerialization]
        private void PrepareBuffer()
        {
            if(pointer == IntPtr.Zero)
            {
                return;
            }
            buffer = new byte[size];
            Marshal.Copy(pointer, buffer, 0, size);
        }

        [PostSerialization]
        private void DisposeBuffer()
        {
            buffer = null;
        }

        [PostDeserialization]
        private void MakeSegment()
        {
            if(Pointer != IntPtr.Zero)
            {
                throw new InvalidOperationException("Unexpected non null pointer during initialization.");
            }
            if(buffer != null)
            {
                Touch();
                Marshal.Copy(buffer, 0, Pointer, size);
                buffer = null;
            }
        }

        [Transient]
        private IntPtr pointer;
        private readonly int offset;
        private readonly int size;
        private byte[] buffer;
    }
}

