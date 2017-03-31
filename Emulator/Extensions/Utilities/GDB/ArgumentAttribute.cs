//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
namespace Emul8.Utilities.GDB
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArgumentAttribute : Attribute
    {
        public char Separator { get; set; }
        public ArgumentEncoding Encoding { get; set; }

        public enum ArgumentEncoding
        {
            DecimalNumber,
            HexNumber,
            HexBytesString, // two hex digits for each byte
            BinaryBytes,
            HexString // two hex digits for every character
        }
    }
}

