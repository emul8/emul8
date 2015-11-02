//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Debug;
using Emul8.Peripherals.CPU;

namespace Emul8.Plugins.TracePlugin.Handlers
{
    public interface IFunctionHandler
    {
        void CallHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> arguments);

        void ReturnHandler(TranslationCPU cpu, uint pc, string functionName, IEnumerable<object> argument);

        IEnumerable<FunctionCallParameter> CallParameters{ get; }

        FunctionCallParameter? ReturnParameter{ get; }
    }

    public class BaseFunctionHandler
    {
        public BaseFunctionHandler(TranslationCPU cpu)
        {
            this.CPU = cpu;
        }

        protected readonly TranslationCPU CPU;
    }
}

