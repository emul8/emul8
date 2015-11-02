//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using Emul8.UserInterface;

namespace Emul8.Peripherals.UART
{
    [Icon("monitor")]
	public interface IUART : IPeripheral
	{
        // This field should be made [Transient] in all implementor classes!
        event Action<byte> CharReceived;
        void WriteChar(byte value);

        uint BaudRate { get; }
        Bits StopBits { get; }
        Parity ParityBit { get; }
	}

    public enum Parity 
    {
        Odd,
        Even,
        None,
        Forced1,
        Forced0
    }

    public enum Bits
    {
        None,
        One,
        Half,
        OneAndAHalf,
        Two
    }
}

