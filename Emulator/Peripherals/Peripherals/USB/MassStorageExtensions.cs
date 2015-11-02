//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.USB
{
    public static class MassStorageExtensions
    {
        public static void PendriveFromFile(this Machine machine, string file, string name, IPeripheralRegister<IUSBPeripheral, USBRegistrationPoint> attachTo, byte port, bool persistent = true)
        {
            // TODO: note that port is here (or is nondefault) only due to bug/deficiency in EHCI
            // i.e. that one cannot register by first free port
            var pendrive = new MassStorage(file, persistent: persistent);
            attachTo.Register(pendrive, new USBRegistrationPoint(port));
            machine.SetLocalName(pendrive, name);
        }
    }
}

