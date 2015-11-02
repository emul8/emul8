//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;

namespace Emul8.Utilities
{
    public interface IBlobProvider
    {
        BlobDescriptor GetBlobDescriptor();
        void BlobIsReady(string fileName, long offset, long length);
    }

    public struct BlobDescriptor
    {
        public Stream Stream { get; set; }
        public long Size { get; set; }
    }
}

