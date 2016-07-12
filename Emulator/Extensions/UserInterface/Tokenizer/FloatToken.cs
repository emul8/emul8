//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Globalization;

namespace Emul8.UserInterface.Tokenizer
{
    public class FloatToken : Token
    {
        public FloatToken(string value) : base(value)
        {
            Value = float.Parse(value, CultureInfo.InvariantCulture);
        }

        public float Value { get; private set; }

        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[FloatToken: Value={0}]", Value);
        }
    }
}

