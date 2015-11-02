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
using Emul8.Logging;
using Emul8.Utilities;
using Emul8.Debug;
using Emul8.Peripherals.CPU;

namespace Emul8.Plugins.TracePlugin.Handlers
{
    public class DefaultFunctionHandler : BaseFunctionHandler, IFunctionHandler
    {
        public DefaultFunctionHandler(TranslationCPU cpu) : base(cpu)
        {
        }

        public void CallHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> arguments)
        {
            Logger.Log(LogLevel.Debug, "Call {0} @ 0x{1:X} ({2})",functionName, pc, arguments.Stringify(", "));
        }

        public void ReturnHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> argument)
        {
            Logger.Log(LogLevel.Debug, "Return from {0} @ 0x{1:X} ({2})",functionName, pc, argument.First());
        }

        public IEnumerable<FunctionCallParameter> CallParameters
        {
            get;
            set;
        }

        public FunctionCallParameter? ReturnParameter
        {
            get;
            set;
        }

    }
}