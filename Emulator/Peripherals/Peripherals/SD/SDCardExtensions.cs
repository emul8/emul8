//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using Emul8.Core.Structure;
using Emul8.Core;
using System;

namespace Emul8.Peripherals.SD
{
    public static class SDCardExtensions
    {
        public static void SdCardFromFile(this Machine machine, string file, IPeripheralRegister<ISDDevice, NullRegistrationPoint> attachTo, bool persistent = true, long? size = null)
        {
            var card = new SDCard(file, size, persistent);
            attachTo.Register(card, NullRegistrationPoint.Instance);
            machine.SetLocalName(card, String.Format("SD card: {0}", file));
        }
    }
}

