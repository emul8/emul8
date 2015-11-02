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
    public class BooleanToken : Token
    {
        public BooleanToken(string value):base(value)
        {
            Value = Boolean.Parse(value);
        }

        public bool Value {get;set;}
        
        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[BooleanToken: Value={0}]", Value);
        }
    }
}

