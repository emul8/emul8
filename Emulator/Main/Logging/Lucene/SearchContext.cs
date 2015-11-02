//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Logging.Lucene
{
    public class SearchContext
    {
        public SearchContext(int count)
        {
            ResultsCount = count;
            CurrentResult = 1;
        }

        public void Advance(Direction direction)
        {
            CurrentResult += (direction == Direction.Backward ? 1 : -1);
        }

        public ulong? PreviousResultId { get; set; }
        public int CurrentResult { get; private set; }
        public int ResultsCount { get; private set; }
    }
}

