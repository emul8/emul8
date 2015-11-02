//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;

namespace Emul8.Peripherals.Input
{

    public interface IAbsolutePositionPointerInput : IPointerInput
    {
        void MoveTo(int x, int y);
        int MaxX {get;}
        int MaxY {get;}

        //These two almost always should equal zero. If you need to provide a 
        //blind area, take a look at IAbsolutePositionPointerInputWithActiveArea.
        int MinX {get;}
        int MinY {get;}
    }
    
}
