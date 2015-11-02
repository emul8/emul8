//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Logging;
using Antmicro.Migrant;

namespace Emul8.Peripherals.Miscellaneous
{
    public class LED : IGPIOReceiver, ILed
    {
        public LED(bool invert = false)
        {
            inverted = invert;
            sync = new object();
        }

        public void OnGPIO(int number, bool value)
        {
            if(number != 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            var stateChanged = StateChanged;
            lock(sync)
            {
                if(stateChanged != null)
                {
                    stateChanged(this, inverted ? !value : value);
                }
                State = inverted ? !value : value;
                this.Log(LogLevel.Noisy, "LED state changed - {0}", inverted ? !value : value);
            }
        }

        public bool State { get; private set; }

        public void Reset()
        {
            // despite apperances, nothing
        }

        [field: Transient]
        public event Action<ILed, bool> StateChanged;

        private bool inverted;

        private readonly object sync;
    }
}

