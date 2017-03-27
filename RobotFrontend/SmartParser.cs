//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Emul8.Robot
{
    internal class SmartParser
    {
        public static SmartParser Instance = new SmartParser();

        public object Parse(string input, Type outputType)
        {
            if(outputType == typeof(string))
            {
                return input;
            }
            var underlyingType = Nullable.GetUnderlyingType(outputType);
            if(underlyingType != null)
            {
                return input == null ? null : Parse(input, underlyingType);
            }
            if(outputType.IsEnum)
            {
                return Enum.Parse(outputType, input);
            }

            NumberStyles style;
            if(input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                style = NumberStyles.HexNumber;
                input = input.Substring(2);
            }
            else
            {
                style = NumberStyles.Integer;
            }

            Delegate parser;
            if(!cache.TryGetValue(outputType, out parser))
            {
                var types = new[] { typeof(string), typeof(NumberStyles) };
                var method = outputType.GetMethod("Parse", types);
                if(method == null)
                {
                    throw new ArgumentException(string.Format("Type \"{0}\" does not have a \"Parse\" method", outputType.Name));
                }

                var delegateType = Expression.GetDelegateType(types.Concat(new[] { method.ReturnType }).ToArray());
                parser = method.CreateDelegate(delegateType);
                cache.Add(outputType, parser);
            }

            return parser.DynamicInvoke(input, style);
        }

        public object[] Parse(string[] input, Type[] outputType)
        {
            var result = new object[Math.Min(input.Length, outputType.Length)];
            for(var i = 0; i < input.Length; i++)
            {
                result[i] = Parse(input[i], outputType[i]);
            }

            return result;
        }

        private SmartParser()
        {
            cache = new Dictionary<Type, Delegate>();
        }

        private readonly Dictionary<Type, Delegate> cache;
    }
}

