//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;

namespace Emul8.Core.Structure
{
    public interface IHasChildren<out T>
    {
        IEnumerable<string> GetNames();
        T TryGetByName(string name, out bool success);
    }

    public static class IHasChildrenHelper
    {
        public static bool TryGetByName<T>(this IHasChildren<T> @this, string name, out T child)
        {
            bool success;
            child = @this.TryGetByName(name, out success);
            return success;
        }
    }
}

