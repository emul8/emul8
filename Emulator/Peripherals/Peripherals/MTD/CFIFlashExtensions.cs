//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using System.IO;
using Emul8.Exceptions;

namespace Emul8.Peripherals.MTD
{
    public static class CFIFlashExtensions
    {
        public static void CFIFlashFromFile(this Machine machine, string fileName, long whereToRegister, string name, Width busWidth = Width.DoubleWord, bool nonPersistent = false, int? size = null)
        {
            CFIFlash flash;
            try
            {
                flash = new CFIFlash(fileName, size ?? (int)new FileInfo(fileName).Length, busWidth, nonPersistent);
            }
            catch(Exception e)
            {
                throw new ConstructionException(String.Format("Could not create object of type {0}", typeof(CFIFlash).Name), e);
            }
            machine.SystemBus.Register(flash, new BusPointRegistration(whereToRegister));
            machine.SetLocalName(flash, name);
        }
    }
}

