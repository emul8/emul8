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
    public class PathToken : Token
    {
        public PathToken(string value) : base(value)
        {
            Value = value.TrimStart('@').Replace(@"\ ", " ");
        }

        public string Value { get; private set; }

        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[PathToken: Value={0}]", Value);
        }
    }
}

