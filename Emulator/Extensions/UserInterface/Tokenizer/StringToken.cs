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
    
    public class StringToken : Token
    {
        public StringToken(string value):base(value)
        {
            var trim = false;
            if(value.StartsWith("\"", StringComparison.Ordinal))
            {
                trim = true;
                value = value.Replace("\\\"", "\"");
            }
            else if(value.StartsWith("'", StringComparison.Ordinal))
            {
                trim = true;
                value = value.Replace("\\\'", "\'");
            }
            if(trim)
            {
                Value = value.Substring(1, value.Length - 2);
            }
            else
            {
                Value = value;
            }
        }

        public string Value { get; protected set; }
        
        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[StringToken: Value={0}]", Value);
        }
    }

}

