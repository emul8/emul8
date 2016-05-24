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
using Emul8.Peripherals.Network;
using System.Net.NetworkInformation;
using System.Linq;
using Emul8.TAPHelper;
using Emul8.Peripherals;
using System.Threading;
using Antmicro.Migrant.Hooks;
using System.IO;
using Emul8.Logging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Emul8.Utilities;
using Emul8.Exceptions;
using Mono.Unix;
using Emul8.Network;
using Antmicro.Migrant;

namespace Emul8.HostInterfaces.Network
{
    public static class TapExtensions
    {
        public static LinuxTapInterface CreateAndGetTAP(this Emulation emulation, string hostInterfaceName, string name, bool persistent = false)
        {
            var onOsX = Misc.IsOnOsX;
            if(onOsX && persistent)
            {
                throw new RecoverableException("Persitent TAP is not available on OS X.");
            }
            var result = new LinuxTapInterface(hostInterfaceName, persistent);
            emulation.HostMachine.AddHostMachineElement(result, name);
            return result;
        }

        public static void CreateTAP(this Emulation emulation, string hostInterfaceName, string name, bool persistent = false)
        {
            CreateAndGetTAP(emulation, hostInterfaceName, name, persistent);
        }
    }

}

