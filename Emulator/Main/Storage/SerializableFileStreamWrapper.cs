//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Antmicro.Migrant;
using System.IO;
using Emul8.Utilities;
using Emul8.Core;
using Antmicro.Migrant.Hooks;

namespace Emul8.Storage
{
    public class SerializableFileStreamWrapper : IDisposable, IBlobProvider
    {
        public SerializableFileStreamWrapper(string underlyingFile)
        {
            Stream = new FileStreamLimitWrapper(new FileStream(underlyingFile, FileMode.OpenOrCreate));
            blobManager = EmulationManager.Instance.CurrentEmulation.BlobManager;
        }

        public void BlobIsReady(string fileName, long offset, long length)
        {
            var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
            Stream = new FileStreamLimitWrapper(fileStream, offset, length);
        }

        public void Dispose()
        {
            Stream.Dispose();
        }

        public BlobDescriptor GetBlobDescriptor()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            return new BlobDescriptor { Stream = Stream, Size = Stream.Length };
        }

        public FileStreamLimitWrapper Stream 
        {
            get { return stream; }
            private set { stream = value; }
        }

        [PreSerialization]
        [PostDeserialization]
        private void OnSerialization()
        {
            blobManager.Register(this);
        }

        private readonly BlobManager blobManager;
        [Transient]
        private FileStreamLimitWrapper stream;
    }
}

