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
    public class HexToken : NumericToken
    {
        public HexToken(string value):base(Convert.ToInt64(value.Split('x')[1], 16).ToString())
        {
        }

        public override string ToString()
        {
            return string.Format("[HexToken: Value={0}]", Value);
        }
    }
}

