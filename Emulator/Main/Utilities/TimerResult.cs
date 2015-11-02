//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Utilities
{
    public class TimerResult
    {
        public override string ToString()
        {
            return string.Format("{2} timer check at {0}, elapsed {1}{3}.", Timestamp.ToLongTimeString(), FromBeginning.ToString(), SequenceNumber.ToOrdinal(), EventName == null ? "" : " " + EventName);
        }

        public int SequenceNumber { get; set; }

        public TimeSpan FromBeginning { get; set; }

        public DateTime Timestamp { get; set; }

        public String EventName { get;set; }
    }
}