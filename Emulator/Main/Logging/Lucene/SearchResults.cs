//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;

namespace Emul8.Logging.Lucene
{
    public struct SearchResults
    {
        public SearchResults(int totalHits, IEnumerable<LogEntry> entries) : this()
        {
            TotalHits = totalHits;
            Entries = entries;
        }

        public ulong FoundId { get; set; }
        public int TotalHits { get; private set; }
        public IEnumerable<LogEntry> Entries { get; private set; }
    }
}

