//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Emul8.Utilities;

namespace Emul8.Core.Structure.Registers
{
    [Flags]
    public enum FieldMode
    {
        Read = 1 << 0,
        Write = 1 << 1,
        Set = 1 << 2,
        Toggle = 1 << 3,
        WriteOneToClear = 1 << 4,
        WriteZeroToClear = 1 << 5,
        ReadToClear = 1 << 6
    }

    public static class FieldModeHelper
    {
        public static bool IsFlagSet(this FieldMode mode, FieldMode value)
        {
            return (mode & value) != 0;
        }

        public static bool IsReadable(this FieldMode mode)
        {
            return (mode & (FieldMode.Read | FieldMode.ReadToClear)) != 0;
        }

        public static bool IsWritable(this FieldMode mode)
        {
            return (mode & (FieldMode.Write | FieldMode.Set | FieldMode.Toggle | FieldMode.WriteOneToClear | FieldMode.WriteZeroToClear)) != 0;
        }

        public static FieldMode WriteBits(this FieldMode mode)
        {
            return mode & ~(FieldMode.Read | FieldMode.ReadToClear);
        }

        public static FieldMode ReadBits(this FieldMode mode)
        {
            return mode & (FieldMode.Read | FieldMode.ReadToClear);
        }

        public static bool IsValid(this FieldMode mode)
        {
            if((mode & (FieldMode.Read | FieldMode.ReadToClear)) == (FieldMode.Read | FieldMode.ReadToClear))
            {
                return false;
            }
            //the assumption that write flags are exclusive is used in BusRegister logic (switch instead of ifs)
            if(BitHelper.GetSetBits((uint)(mode & (FieldMode.Write | FieldMode.Set | FieldMode.Toggle | FieldMode.WriteOneToClear | FieldMode.WriteZeroToClear))).Count > 1)
            {
                return false;
            }
            return true;
        }
    }
}
