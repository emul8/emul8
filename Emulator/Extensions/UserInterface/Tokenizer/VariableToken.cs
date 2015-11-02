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
    public class VariableToken : Token
    {
        public VariableToken(string value):base(value)
        {
            Value = value.TrimStart('$');
        }

        public string Value { get; private set; }
        
        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[VariableToken: Value={0}]", Value);
        }
    }
}

