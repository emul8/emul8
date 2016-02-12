// 
//  Copyright (c) Antmicro
// 
//  This file is part of the Emul8 project.
//  Full license details are defined in the 'LICENSE' file.
// 
using System;

namespace Emul8.Peripherals.Bus
{
    internal class BusHookHandler
    {
        public BusHookHandler(Action<long, Width> action, Width width, Action updateContext)
        {
            this.action = action;
            this.width = width;
            this.updateContext = updateContext;
        }        

        public void Invoke(long currentAddress, Width currentWidth)
        {
            if(updateContext != null)
            {
                updateContext();
            }
            if((currentWidth & width) != 0)
            {
                action(currentAddress, currentWidth);
            }
        }

        public bool ContainsAction(Action<long, Width> actionToTest)
        {
            return action == actionToTest;
        }

        private readonly Action<long, Width> action;
        private readonly Width width;
        private readonly Action updateContext;
    }
}

