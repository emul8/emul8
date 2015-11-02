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
    public class MultilineStringToken : StringToken
    {
        public MultilineStringToken(string value) : base(value)
        {
            if(value.StartsWith(@"""""""", StringComparison.Ordinal))
            {
                Value = value.Substring(3, value.Length - 6);
            }
            else
            {
                Value = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[MultilineString: Value={0}]", Value);
        }
    }
}

