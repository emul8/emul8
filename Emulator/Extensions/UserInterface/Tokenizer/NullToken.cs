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
    public class NullToken:Token
    {
        public NullToken(string value):base(value)
        {
        }

        public object Value {get{return null;}}

        public override object GetObjectValue()
        {
            return Value;
        }
    }
}

