//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Debug
{
    public struct FunctionCallParameter
    {
        static FunctionCallParameter()
        {
            IgnoredParameter = new FunctionCallParameter { Type = FunctionCallParameterType.Ignore };
        }

        public FunctionCallParameterType Type;
        public int NumberOfElements;

        public static FunctionCallParameter IgnoredParameter;
    }
    
}
