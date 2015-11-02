//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Peripherals
{
    public class BasicIAnalyzableBackend<T> : IAnalyzableBackend<T> where T: IAnalyzable
    {
        public void Attach(T analyzableElement)
        {
            AnalyzableElement = analyzableElement;
        }

        public IAnalyzable AnalyzableElement { get; private set; }
    }
}

