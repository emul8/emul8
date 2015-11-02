//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Emul8.Peripherals.Bus.Wrappers
{
    public class RegisterMapper
    {
        public RegisterMapper(Type peripheralType)
        {
            var types = peripheralType.GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var interestingEnums = new List<Type>();

            var enumsWithAttribute = types.Where(t => t.GetCustomAttributes(false).Any(x => x is RegistersDescriptionAttribute));
            if (enumsWithAttribute != null)
            {
                interestingEnums.AddRange(enumsWithAttribute);
            }
            interestingEnums.AddRange(types.Where(t => t.BaseType == typeof(Enum) && t.Name.IndexOf("register", StringComparison.CurrentCultureIgnoreCase) != -1));

            foreach (var type in interestingEnums)
            {
                foreach (var value in type.GetEnumValues())
                {
                    var l = Convert.ToInt64(value);
                    var s = Enum.GetName(type, value);

                    if (!map.ContainsKey(l))
                    {
                        map.Add(l, s);
                    }
                }
            }
        }

        public string ToString(long offset, string format)
        {
            string name;
            if (!map.ContainsKey(offset))
            {
                var closestCandidates = map.Keys.Where(k => k < offset).ToList();
                if (closestCandidates.Count > 0)
                {
                    var closest = closestCandidates.Max();
                    name = string.Format("{0}+0x{1:x}", map[closest], offset - closest);
                }
                else
                {
                    name = "unknown";
                }
            }
            else
            {
                name = map[offset];
            }

            return string.Format(format, name);
        }

        private readonly Dictionary<long, string> map = new Dictionary<long, string>();

        [AttributeUsage(AttributeTargets.Enum)]
        public class RegistersDescriptionAttribute : Attribute { }
    }
}

