//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.Input;

namespace Emul8.Extensions.Analyzers.Video.Handlers
{
    internal class RelativePointerHandler : PointerHandler
    {
        public RelativePointerHandler(IRelativePositionPointerInput input) : base(input)
        {
        }

        public override void PointerMoved(int x, int y, int dx, int dy)
        {
            ((IRelativePositionPointerInput)input).MoveBy(dx, dy);
        }
    }
}

