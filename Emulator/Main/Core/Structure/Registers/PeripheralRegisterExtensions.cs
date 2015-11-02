//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;

namespace Emul8.Core.Structure.Registers
{
    public static class PeripheralRegisterExtensions
    {
        /// <summary>
        /// Fluent API for flag field creation. For parameters see <see cref="PeripheralRegister.DefineValueField"/>.
        /// </summary>
        /// <returns>This register with a defined flag.</returns>
        public static T WithFlag<T>(this T register, int position, FieldMode mode = FieldMode.Read | FieldMode.Write, Action<bool, bool> readCallback = null,
            Action<bool, bool> writeCallback = null, Action<bool, bool> changeCallback = null, Func<bool, bool> valueProviderCallback = null, string name = null) where T : PeripheralRegister
        {
            register.DefineFlagField(position, mode, readCallback, writeCallback, changeCallback, valueProviderCallback, name);
            return register;
        }

        /// <summary>
        /// Fluent API for value field creation. For parameters see <see cref="PeripheralRegister.DefineValueField"/>.
        /// </summary>
        /// <returns>This register with a defined value field.</returns>
        public static T WithValueField<T>(this T register, int position, int width, FieldMode mode = FieldMode.Read | FieldMode.Write, Action<uint, uint> readCallback = null,
            Action<uint, uint> writeCallback = null, Action<uint, uint> changeCallback = null, Func<uint, uint> valueProviderCallback = null, string name = null) where T : PeripheralRegister
        {
            register.DefineValueField(position, width, mode, readCallback, writeCallback, changeCallback, valueProviderCallback, name);
            return register;
        }

        /// <summary>
        /// Fluent API for enum field creation. For parameters see <see cref="PeripheralRegister.DefineValueField"/>.
        /// </summary>
        /// <returns>This register with a defined enum field.</returns>
        public static R WithEnumField<R, T>(this R register, int position, int width, FieldMode mode = FieldMode.Read | FieldMode.Write, Action<T, T> readCallback = null,
            Action<T, T> writeCallback = null, Action<T, T> changeCallback = null, Func<T, T> valueProviderCallback = null, string name = null) where R : PeripheralRegister
                where T : struct, IConvertible
        {
            register.DefineEnumField<T>(position, width, mode, readCallback, writeCallback, changeCallback, valueProviderCallback, name);
            return register;
        }

        /// <summary>
        /// Fluent API for tagged field creation. For parameters see <see cref="PeripheralRegister.DefineValueField"/>.
        /// </summary>
        /// <returns>This register with a defined tag field.</returns>
        public static T WithTag<T>(this T register, string name, int position, int width) where T : PeripheralRegister
        {
            register.Tag(name, position, width);
            return register;
        }
    }
}


