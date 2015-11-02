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
    // You might ask, why there is CustomDateTime calculating elapsed time in such a strange manner?
    // This is because getting DateTime.Now on mono is veeeeeeeeery slow and this is much faster 
    public static class CustomDateTime
    {
        public static DateTime Now { get { return DateTime.UtcNow.Add(timeDifference); } }

        static CustomDateTime()
        {
            var ournow = DateTime.Now;
            var utcnow = TimeZoneInfo.ConvertTimeToUtc(ournow);

            timeDifference = ournow - utcnow;
        }

        private static readonly TimeSpan timeDifference;
    }
}

