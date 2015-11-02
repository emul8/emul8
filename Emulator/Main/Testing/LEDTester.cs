//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Threading;
using Emul8.Peripherals.Miscellaneous;

namespace Emul8.Testing
{
    public static class LEDTesterExtenions
    {
        public static void CreateLEDTester(this Emulation emulation, string name, ILed led)
        {
            emulation.ExternalsManager.AddExternal(new LEDTester(led), name);
        }
    }

    public class LEDTester : IExternal
    {
        public LEDTester(ILed led)
        {
            this.led = led;
        }

        public LEDTester AssertState(bool state, int timeout = 0)
        {
            ManualResetEvent ev = null;
            Action<ILed, bool> method = null;

            if (timeout != 0)
            {
                ev = new ManualResetEvent(false);
                method = (s,o) => ev.Set();

                led.StateChanged += method;
            }

            if (led.State != state)
            {
                if (!TimeoutExecutor.WaitForEvent(ev, timeout))
                {
                    if (timeout != 0)
                    {
                        led.StateChanged -= method;
                    }
                    throw new InvalidOperationException(string.Format("LED assertion not met. Was {0}, should be {1}.", led.State, state));
                }
            }

            if (timeout != 0)
            {
                led.StateChanged -= method;
            }

            return this;
        }

        private readonly ILed led;
    }
}

