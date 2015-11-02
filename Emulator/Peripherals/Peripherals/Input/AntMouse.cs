//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using System.Linq;

namespace Emul8.Peripherals.Input
{
    public class AntMouse : IDoubleWordPeripheral
    {
        public AntMouse()
        {
            IRQ = new GPIO();
        }

        public uint ReadDoubleWord(long offset)
        {
            switch((Registers)offset)
            {
            case Registers.X:
                return (uint)x;
            case Registers.Y:
                return (uint)y;
            case Registers.LeftButton:
                return leftButton ? 1u : 0u;
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }
        }

        public GPIO IRQ { get; private set; }

        public void WriteDoubleWord(long offset, uint value)
        {
            switch((Registers)offset)
            {
            case Registers.InterruptHandled:
                IRQ.Unset();
                return;
            }
            this.LogUnhandledWrite(offset, value);
        }

        public void Reset()
        {
            x = 0;
            y = 0;
            leftButton = false;
            Refresh();
        }

        public void Move(int newx, int newy)
        {
            x = newx;
            y = newy;
            if(leftButton)
            {
                Refresh();
            }
        }

        public void MouseDown()
        {
            leftButton = true;
            Refresh();
        }

        public void MouseUp()
        {
            leftButton = false;
            Refresh();
        }

        private void Refresh()
        {
            IRQ.Set();
        }

        private enum Registers
        {
            X = 0,
            Y = 4,
            LeftButton = 8,
            InterruptHandled = 12
        }

        private int x;
        private int y;
        private bool leftButton;
    }
}

