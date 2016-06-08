//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Core
{
    public class ObjectCreator
    {
        static ObjectCreator()
        {
            Instance = new ObjectCreator();
        }

        public static ObjectCreator Instance { get; private set; }

        public Context OpenContext()
        {
            var context = new Context(this);
            contexts.Push(context);
            return context;
        }

        public object Spawn(Type type)
        {
            foreach(var constructor in type.GetConstructors())
            {
                var @params = constructor.GetParameters();
                var args = new object[@params.Length];
                var success = true;
                for(int i = 0; i < args.Length; i++)
                {
                    var surrogate = GetSurrogate(@params[i].ParameterType);
                    if(surrogate == null)
                    {
                        success = false;
                        break;
                    }

                    args[i] = surrogate;
                }

                // try next constructor
                if(!success)
                {
                    continue;
                }

                return Activator.CreateInstance(type, args);
            }

            // if no constructor was matched, throw exception
            throw new ArgumentException(string.Format("Couldn't spawn object of type: {0}\nAvailable surrogate types are:\n{1}", type.FullName, 
                string.Join("\n", contexts.SelectMany(x => x.Types).Distinct().Select(x => x.FullName))));
        }

        public T GetSurrogate<T>()
        {
            return (T)GetSurrogate(typeof(T));
        }

        public object GetSurrogate(Type type)
        {
            return contexts.Select(x => x.GetSurrogate(type)).FirstOrDefault(x => x != null);
        }

        protected ObjectCreator()
        {
            contexts = new Stack<Context>();
        }

        private void CloseContext(Context context)
        {
            if(context != contexts.Peek())
            {
                throw new ArgumentException();
            }

            contexts.Pop();
        }

        private readonly Stack<Context> contexts;

        public class Context : IDisposable
        {
            public Context(ObjectCreator creator)
            {
                objectCreator = creator;
                surrogates = new Dictionary<Type, object>();
            }
            
            public void RegisterSurrogate(Type type, object obj)
            {
                surrogates.Add(type, obj);
            }

            public object GetSurrogate(Type type)
            {
                object result;
                surrogates.TryGetValue(type, out result);
                return result;
            }

            public void Close()
            {
                objectCreator.CloseContext(this);
            }

            public void Dispose()
            {
                Close();
            }

            public IEnumerable<Type> Types { get { return surrogates.Keys; } }

            private readonly ObjectCreator objectCreator;
            private readonly Dictionary<Type, object> surrogates;
        }
    }
}

