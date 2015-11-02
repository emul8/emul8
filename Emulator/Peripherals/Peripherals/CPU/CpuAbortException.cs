//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.CPU
{
    public class CpuAbortException : Exception
    {
        public CpuAbortException()
        {
        }
        

        public CpuAbortException(string message) : base(message)
        {
        }
        

        public CpuAbortException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
    }
}

