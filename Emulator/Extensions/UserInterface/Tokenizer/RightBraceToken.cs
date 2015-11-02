//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.UserInterface.Tokenizer
{
    public class RightBraceToken : Token 
    {
        public RightBraceToken(string token) : base(token)
        {
        }

        public override object GetObjectValue()
        {
            return OriginalValue;
        }

        public override string ToString()
        {
            return string.Format("[RightBraceToken]");
        }
    }
}

