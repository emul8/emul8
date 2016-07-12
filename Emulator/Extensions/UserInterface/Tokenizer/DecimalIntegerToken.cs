//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//


namespace Emul8.UserInterface.Tokenizer
{
    public class DecimalIntegerToken : Token
    {
        public DecimalIntegerToken(string value) : base(value)
        {
            Value = long.Parse(value);
        }

        public long Value { get; private set; }

        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[NumericToken: Value={0}]", Value);
        }
    }
}

