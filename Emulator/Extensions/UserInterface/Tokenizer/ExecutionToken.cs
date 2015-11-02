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
    public class ExecutionToken : Token
    {
        public ExecutionToken(string value) : base(value)
        {
            if(value.StartsWith("`", StringComparison.Ordinal))
            {
                Value = value.Substring(1, value.Length - 2);
            }
            else
            {
                Value = value;
            }
        }
        public string Value {get;set;}

        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[ExecutionToken: Value={0}]", Value);
        }
    }
}

