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
    public class ConditionalEqualityToken : EqualToken
    {
        public ConditionalEqualityToken(string value) : base(value)
        {
        }

        public override string ToString()
        {
            return string.Format("[ConditionalEqualityToken: Value={0}]", Value);
        }
    }
}

