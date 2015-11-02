//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Emul8.UserInterface;

namespace Emul8.Peripherals.SD
{
    [Icon("sd")]
    public interface ISDDevice : IPeripheral, IDisposable
    {
        uint GoIdleState();
        uint SendOpCond();
        uint SendStatus(bool dataTransfer);
        ulong SendSdConfigurationValue();
        uint SetRelativeAddress();
        uint SelectDeselectCard();
        uint SendExtendedCardSpecificData();
        uint[] AllSendCardIdentification();
        uint AppCommand(uint argument);
        uint Switch();
        uint[] SendCardSpecificData();
        uint SendAppOpCode(uint argument);
        byte[] ReadData(long offset, int size);
        void WriteData(long offset, int size, byte[] data);
    }
}

