//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Robot
{
    public class RobotFrameworkKeywordAttribute : Attribute
    {
        public RobotFrameworkKeywordAttribute()
        {
        }

        public RobotFrameworkKeywordAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

