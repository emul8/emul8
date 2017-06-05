﻿//
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
using System.Reflection;

namespace Emul8.Utilities
{
    public class SmartParser
    {
        public static SmartParser Instance = new SmartParser();

        public bool TryParse(string input, Type outputType, out object result)
        {
            try
            {
                result = Parse(input, outputType);
                return true;
            }
            catch(TargetInvocationException)
            {
                result = null;
                return false;
            }
        }

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

            Delegate parser;
            if(input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(2);

                if(!hexCache.TryGetValue(outputType, out parser))
                {
                    parser = GetParseMethodDelegate(outputType, new[] { typeof(string), typeof(NumberStyles), typeof(CultureInfo) });
                    hexCache.Add(outputType, parser);
                }

                return parser.DynamicInvoke(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else
            {
                if(!cache.TryGetValue(outputType, out parser))
                {
                    parser = GetParseMethodDelegate(outputType, new[] { typeof(string), typeof(CultureInfo) });
                    cache.Add(outputType, parser);
                }

                return parser.DynamicInvoke(input, CultureInfo.InvariantCulture);
            }
        }

        private static Delegate GetParseMethodDelegate(Type type, Type[] parameters)
        {
            var method = type.GetMethod("Parse", parameters);
            if(method == null)
            {
                throw new ArgumentException(string.Format("Type \"{0}\" does not have a \"Parse\" method with the requested parameters", type.Name));
            }

            var delegateType = Expression.GetDelegateType(parameters.Concat(new[] { method.ReturnType }).ToArray());
            return method.CreateDelegate(delegateType);
        }

        private SmartParser()
        {
            cache = new Dictionary<Type, Delegate>();
            hexCache = new Dictionary<Type, Delegate>();
        }

        private readonly Dictionary<Type, Delegate> cache;
        private readonly Dictionary<Type, Delegate> hexCache;
    }
}

