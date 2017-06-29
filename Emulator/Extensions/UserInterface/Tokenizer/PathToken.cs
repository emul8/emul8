//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emul8.UserInterface.Tokenizer
{
    public class PathToken : Token
    {
        public PathToken(string value) : base(value)
        {
            Value = value.TrimStart('@').Replace(@"\ ", " ");
            fullPaths = new[] { value };
        }

        public string Value { get; private set; }

        public void SetPossiblePrefixes(IEnumerable<string> prefixes)
        {
            fullPaths = prefixes.Select(x => Path.Combine(x, Value)).ToArray();
        }

        public IEnumerable<string> GetPossiblePaths()
        {
            return fullPaths;
        }

        public override object GetObjectValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return string.Format("[PathToken: Value={0}]", Value);
        }

        private string[] fullPaths;
    }
}

