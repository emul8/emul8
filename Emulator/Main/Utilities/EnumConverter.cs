//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq.Expressions;

namespace Emul8.Utilities
{
    static class EnumConverter<TEnum> where TEnum : struct, IConvertible
    {
        public static readonly Func<uint, TEnum> ToEnum = GenerateEnumConverter();

        public static readonly Func<TEnum, uint> ToUInt = GenerateLongConverter();

        private static Func<uint, TEnum> GenerateEnumConverter()
        {
            var parameter = Expression.Parameter(typeof(uint));
            var dynamicMethod = Expression.Lambda<Func<uint, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile();
        }

        private static Func<TEnum, uint> GenerateLongConverter()
        {
            var parameter = Expression.Parameter(typeof(TEnum));
            var dynamicMethod = Expression.Lambda<Func<TEnum, uint>>(
                Expression.Convert(parameter, typeof(uint)),
                parameter);
            return dynamicMethod.Compile();
        }
    }

}

