//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core.Structure
{
    public class MACRepository
    {
        public MACAddress GenerateUniqueMAC()
        {
            lock(currentMAClock)
            {
                var result = currentMAC;
                currentMAC = currentMAC.Next();
                return result;
            }
        }

        private MACAddress currentMAC = MACAddress.Parse("00:00:00:00:00:02");
        private readonly object currentMAClock = new object();
    }
}

