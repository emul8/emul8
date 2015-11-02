//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Core
{
    public sealed class GPIOConnection
    {
        public int SourceNumber
        {
            get;
            private set;
        }

        public GPIOEndpoint GPIOEndpoint
        {
            get;
            private set;
        }
        public GPIOConnection(int sourceNumber, GPIOEndpoint endpoint)
        {
            this.SourceNumber = sourceNumber;
            this.GPIOEndpoint = endpoint;
        }
    }
}

