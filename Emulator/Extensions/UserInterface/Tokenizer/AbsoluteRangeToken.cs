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
using Emul8.Exceptions;

namespace Emul8.UserInterface.Tokenizer
{
    public class AbsoluteRangeToken : RangeToken
    {
        public AbsoluteRangeToken(string value) : base(value)
        {
            var trimmed = value.TrimStart('<').TrimEnd('>');
            var split = trimmed.Split(',').Select(x => x.Trim()).ToArray();
            var resultValues = ParseNumbers(split);

            Range temp;
            // we need a size, so we add 1 to range.end - range.being result. Range <0x0, 0xFFF> has a size of 0x1000.
            if(!Range.TryCreate(resultValues[0], resultValues[1] - resultValues[0] + 1, out temp))
            {
                throw new RecoverableException("Could not create range. Size has to be non-negative.");
            }
            Value = temp;
        }
    }
}
