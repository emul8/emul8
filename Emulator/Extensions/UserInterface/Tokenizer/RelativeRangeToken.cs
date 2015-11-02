//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.Linq;
using Emul8.Core;

namespace Emul8.UserInterface.Tokenizer
{
    public class RelativeRangeToken : RangeToken
    {
        public RelativeRangeToken(string value) : base(value)
        {
            var trimmed = value.TrimStart('<').TrimEnd('>');
            var split = trimmed.Split(new []{ ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            var resultValues = ParseNumbers(split);

            Value = new Range(resultValues[0], resultValues[1]);
        }
    }
}
