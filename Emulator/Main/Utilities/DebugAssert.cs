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
    class AssertException : Exception
    {
        public AssertException()
        {
        }

        public AssertException(string message) : base(message)
        {
        }

        public AssertException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Simple class that throws exception when the assertion fails.
    /// This class' methods should be only in "debug mode", so they are defined only when
    /// DEBUG symbol is defined, to avoid accidental condition or message parameter
    /// evaluations in release builds.
    /// </summary>
    static class DebugAssert
    {
        #if DEBUG
        static public void Assert(bool condition, string message)
        {
            if(!condition)
            {
                throw new AssertException(String.Concat("Assert failed: ",message));
            }
        }

        static public void AssertFalse(bool condition, string message)
        {
            Assert(!condition, message);
        }
        #endif
    }
}

