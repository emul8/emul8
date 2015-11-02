//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Utilities
{
    public abstract class TypeSorter
    {
        public static void Sort(Type[] t)
        {
            Array.Sort(t, Compare);
        }

        public static int Compare(Type tone, Type ttwo)
        {
            var t1tot2 = tone.IsAssignableFrom(ttwo);
            var t2tot1 = ttwo.IsAssignableFrom(tone);

            if(t1tot2 && !t2tot1)
            {
                return 1;
            }
            else if(t2tot1 && !t1tot2)
            {
                return -1;
            }

            return 0;
        }
    }
}

