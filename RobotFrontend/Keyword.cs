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

        public object Execute(string[] arguments)
        {
            var obj = manager.GetOrCreateObject(methodInfo.DeclaringType);
            var parameters = methodInfo.GetParameters();

            var parsedArguments = parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]) 
                ? new object[] { arguments } 
                : SmartParser.Instance.Parse(arguments, parameters.Select(x => x.ParameterType).ToArray());

            if(parameters.Length > parsedArguments.Length && parameters[parsedArguments.Length].HasDefaultValue)
            {
                parsedArguments = parsedArguments.Union(parameters.Skip(parsedArguments.Length).Select(x => x.DefaultValue)).ToArray();
            }
            else if(parameters.Length != parsedArguments.Length)
            {
                throw new KeywordException("Wrong number of arguments passed.");
            }

            return methodInfo.Invoke(obj, parsedArguments);
        }

        private readonly MethodInfo methodInfo;
        private readonly KeywordManager manager;
    }
}

