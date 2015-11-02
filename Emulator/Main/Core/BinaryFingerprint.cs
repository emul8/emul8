//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace Emul8.Core
{
    public sealed class BinaryFingerprint
    {
        public BinaryFingerprint(string file)
        {
            using(var md5 = MD5.Create())
            using(var stream = File.OpenRead(file))
            {
                md5.ComputeHash(stream);
                hash = md5.Hash;
            }
            FileName = Path.GetFullPath(file);
        }

        public string FileName { get; private set; }

        public string Hash
        {
            get
            {
                return hash.Select(x => x.ToString("x2")).Aggregate((x, y) => x + y);
            }
        }

        public override string ToString()
        {
            return string.Format("Binary {0}: {1}", FileName, Hash);
        }

        private readonly byte[] hash;
    }
}

