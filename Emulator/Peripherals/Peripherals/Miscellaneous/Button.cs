//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using System;

namespace Emul8.Peripherals.Miscellaneous
{
    public class Button : IPeripheral, IGPIOSender
    {
        public Button()
        {
            IRQ = new GPIO();
        }

        public void PressAndRelease()
        {
            IRQ.Set();
            IRQ.Unset();
            OnStateChange(true);
            OnStateChange(false);
        }

        public void Toggle()
        {
            if (Pressed)
            {
                IRQ.Unset();
                Pressed = false;
                OnStateChange(false);
            }
            else
            {
                IRQ.Set();
                Pressed = true;
                OnStateChange(true);
            }
        }

        private void OnStateChange(bool pressed)
        {
            var sc = StateChanged;
            if (sc != null)
            {
                sc(pressed);
            }
        }

        public GPIO IRQ { get; private set; }

        public event Action<bool> StateChanged;

        public bool Pressed { get; private set; }

        #region IPeripheral implementation

        public void Reset()
        {
            // despite apperances, nothing
        }

        #endregion
    }
}

