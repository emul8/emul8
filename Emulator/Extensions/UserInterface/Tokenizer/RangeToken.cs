//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Linq;

namespace Emul8.UserInterface.Tokenizer
{
    public abstract class RangeToken : Token
    {
        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[RangeToken: Value={0}]", Value);
        }

        public Range Value { get; protected set; }

        protected RangeToken(string value) : base(value)
        {
        }

        protected long[] ParseNumbers(string[] input)
        {
            var resultValues = new long[2];
            for(var i = 0; i < input.Length; ++i)
            {
                resultValues[i] = input[i].Contains('x')
                    ? Convert.ToInt64(input[i].Split('x')[1], 16)
                    : resultValues[i] = long.Parse(input[i]);

            }
            return resultValues;
        }
    }
}

