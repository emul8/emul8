//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.OptionsParser;

namespace Emul8.Robot
{
    internal class Options
    {
        [Name("port"), PositionalArgument(0), DefaultValueAttribute(9999)]
        public int Port { get; set; }
    }
}

