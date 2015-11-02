//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Logging.Backends;

namespace Emul8.Logging.Lucene
{
    public class Range
    {
        public ulong? MinimalId { get; set; }
        public ulong? MaximalId { get; set; }

        public string GenerateQueryString()
        {
            return string.Format("{0}:[{1} TO {2}]", LuceneLoggerBackend.IdFieldName, MinimalId ?? 0, MaximalId ?? ulong.MaxValue);
        }
    }
}

