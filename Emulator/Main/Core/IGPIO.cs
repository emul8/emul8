//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Core
{
    public interface IGPIO
    {
        bool IsSet { get; }
        void Set(bool value);
        // TODO: this method could be simulated by calling <<Set(!IsSet)>>, but this requires locking ...
        void Toggle();

        bool IsConnected { get; }
        void Connect(IGPIOReceiver destination, int destinationNumber);
        void Disconnect();

        GPIOEndpoint Endpoint { get; }
    }

    public static class IGPIOExtensions
    {
        public static void Set(this IGPIO gpio)
        {
            gpio.Set(true);
        }

        public static void Unset(this IGPIO gpio)
        {
            gpio.Set(false);
        }

        public static void Blink(this IGPIO gpio)
        {
            gpio.Set();
            gpio.Unset();
        }
    }
}

