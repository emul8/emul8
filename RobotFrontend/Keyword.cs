//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;
using System.Reflection;

namespace Emul8.Robot
{
    internal class Keyword
    {
        public Keyword(KeywordManager manager, MethodInfo info)
        {
            this.manager = manager;
            methodInfo = info;
        }

        public bool TryMatchArguments(string[] arguments, out object[] parsedArguments)
        {
            var parameters = methodInfo.GetParameters();

            parsedArguments = parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]) 
                ? new object[] { arguments } 
                : SmartParser.Instance.Parse(arguments, parameters.Select(x => x.ParameterType).ToArray());

            if(parameters.Length > parsedArguments.Length && parameters[parsedArguments.Length].HasDefaultValue)
            {
                parsedArguments = parsedArguments.Concat(parameters.Skip(parsedArguments.Length).Select(x => x.DefaultValue)).ToArray();
            }

            return parameters.Length == parsedArguments.Length;
        }

        public object Execute(object[] arguments)
        {
            var obj = manager.GetOrCreateObject(methodInfo.DeclaringType);
            return methodInfo.Invoke(obj, arguments);
        }

        public int NumberOfArguments 
        {
            get
            {
                return methodInfo.GetParameters().Length;
            }
        }

        private readonly MethodInfo methodInfo;
        private readonly KeywordManager manager;
    }
}

