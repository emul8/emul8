//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Emul8.Robot
{
    internal class KeywordManager : IDisposable
    {
        public KeywordManager()
        {
            keywords = new Dictionary<string, Keyword>();
            objects = new Dictionary<Type, IRobotFrameworkKeywordProvider>();
        }

        public void Register(Type t)
        {
            if(!typeof(IRobotFrameworkKeywordProvider).IsAssignableFrom(t))
            {
                return;
            }
            
            foreach(var method in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttributes<RobotFrameworkKeywordAttribute>().SingleOrDefault();
                if(attr != null)
                {
                    var keyword = attr.Name ?? method.Name;
                    keywords.Add(keyword, new Keyword(this, method));
                }
            }
        }

        public object GetOrCreateObject(Type declaringType)
        {
            IRobotFrameworkKeywordProvider result;
            if(!objects.TryGetValue(declaringType, out result))
            {
                result = (IRobotFrameworkKeywordProvider)Activator.CreateInstance(declaringType);
                objects.Add(declaringType, result);
            }

            return result;
        }

        public bool TryGetKeyword(string keyword, out Keyword result)
        {
            return keywords.TryGetValue(keyword, out result);
        }

        public string[] GetRegisteredKeywords()
        {
            return keywords.Keys.ToArray();
        }

        public void Dispose()
        {
            foreach(var obj in objects)
            {
                obj.Value.Dispose();
            }
        }

        private readonly Dictionary<string, Keyword> keywords;
        private readonly Dictionary<Type, IRobotFrameworkKeywordProvider> objects;
    }
}

