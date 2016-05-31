//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Network;
using Emul8.Utilities;
using Emul8.Exceptions;

namespace Emul8.HostInterfaces.Network
{
    public static class TapExtensions
    {
        public static IMACInterface CreateAndGetTap(this Emulation emulation, string hostInterfaceName, string name, bool persistent = false)
        {
            var onOsX = Misc.IsOnOsX;
            if(onOsX && persistent)
            {
                throw new RecoverableException("Persitent TAP is not available on OS X.");
            }
            var result = onOsX ? (ITapInterface)new OsXTapInterface(hostInterfaceName) : new LinuxTapInterface(hostInterfaceName, persistent);
            emulation.HostMachine.AddHostMachineElement(result, name);
            return result;
        }

        public static void CreateTap(this Emulation emulation, string hostInterfaceName, string name, bool persistent = false)
        {
            CreateAndGetTap(emulation, hostInterfaceName, name, persistent);
        }
    }

}

