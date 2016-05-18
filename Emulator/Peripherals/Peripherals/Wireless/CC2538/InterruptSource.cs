//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Peripherals.Wireless.CC2538
{
    internal enum InterruptSource
    {
        StartOfFrameDelimiter,
        FifoP,
        SrcMatchDone,
        SrcMatchFound,
        FrameAccepted,
        RxPktDone,
        RxMaskZero,

        TxAckDone,
        TxDone,
        RfIdle,
        CommandStrobeProcessorManualInterrupt,
        CommandStrobeProcessorStop,
        CommandStrobeProcessorWait
    }
}
