//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;
using System.Linq;
using System.Collections.Generic;
using Emul8.Debug;
using Emul8.Peripherals.CPU;

namespace Emul8.Plugins.TracePlugin.Handlers
{
    public class PrintfHandler : BaseFunctionHandler, IFunctionHandler
    {
        public PrintfHandler(TranslationCPU cpu) : base(cpu)
        {
        }

        public void CallHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> arguments)
        {
            Logger.LogAs(this, LogLevel.Warning, arguments.First().ToString());
        }

        public void ReturnHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> argument)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FunctionCallParameter> CallParameters
        {
            get
            {
                return callParameters;
            }
        }

        public FunctionCallParameter? ReturnParameter
        {
            get
            {
                return null;
            }
        }

        private static readonly IEnumerable<FunctionCallParameter> callParameters = new []{ new FunctionCallParameter{ Type = FunctionCallParameterType.String } };
    }
}

