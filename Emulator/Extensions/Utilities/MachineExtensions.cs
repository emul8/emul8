//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.Config.Devices;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using System.Collections.Generic;
using Machine = Emul8.Core.Machine;
using Emul8.Core.Structure;
using System.IO;
using FdtSharp;
using Emul8.Logging;
using System.Text;
using Emul8.Exceptions;

namespace Emul8.Utilities
{
    public static class MachineExtensions
    {
        public static void LoadPeripherals(this Machine machine, String fileName)
        {
            if(!File.Exists(fileName))
            {
                throw new RecoverableException("Cannot load devices configuration from file {0} as it does not exist.".FormatWith(fileName));
            }
            new DevicesConfig(File.ReadAllText(fileName), machine);
        }

        public static void LoadPeripheralsFromString(this Machine machine, String text)
        {
            new DevicesConfig(text, machine);
        }

        public static void LoadAtags(this SystemBus bus, String bootargs, uint memorySize, uint beginAddress)
        {
            var atags = Misc.CreateAtags(bootargs, memorySize);
            //Fill ATAGs
            var addr = beginAddress;
            foreach(var elem in atags)
            {
                bus.WriteDoubleWord(addr, elem);
                addr += 4;
            }

        }

        public static void LoadFdt(this SystemBus sysbus, string file, long address, string bootargs = null, bool append = true, string excludedNodes = "")
        {
            var fdtBlob = File.ReadAllBytes(file);
            if(bootargs == null)
            {
                sysbus.WriteBytes(fdtBlob, address, true);
                return;
            }
            var fdt = new FlattenedDeviceTree(fdtBlob);
            var chosenNode = fdt.Root.Subnodes.FirstOrDefault(x => x.Name == "chosen");
            if(chosenNode == null)
            {
                chosenNode = new TreeNode { Name = "chosen" };
                fdt.Root.Subnodes.Add(chosenNode);
            }
            var bootargsProperty = chosenNode.Properties.FirstOrDefault(x => x.Name == "bootargs");
            if(bootargsProperty == null)
            {
                bootargsProperty = new Property("bootargs", new byte[] { 0 });
                chosenNode.Properties.Add(bootargsProperty);
            }
            var oldBootargs = bootargsProperty.GetDataAsString();
            if(append)
            {
                bootargs = oldBootargs + bootargs;
            }
            if(oldBootargs != bootargs)
            {
                sysbus.DebugLog("Bootargs altered from '{0}' to '{1}'.", oldBootargs, bootargs);
            }
            bootargsProperty.PutDataAsString(bootargs);

            var excludedNodeNames = excludedNodes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] disabledValue = Encoding.ASCII.GetBytes("disabled");
            foreach(var deviceName in excludedNodeNames)
            {
                TreeNode node = fdt.Root.Descendants.FirstOrDefault(x => x.Name == deviceName);
                if(node == null)
                {
                    throw new RecoverableException(String.Format("Device {0} not found.", deviceName));
                }
                else
                {
                    Property statusProperty = node.Properties.FirstOrDefault(x => x.Name == "status");
                    if(statusProperty != null)
                    {
                        node.Properties.Remove(statusProperty);
                    }
                    statusProperty = new Property("status", disabledValue);
                    node.Properties.Add(statusProperty);
                }
            }

            fdtBlob = fdt.GetBinaryBlob();
            sysbus.WriteBytes(fdtBlob, address, true);
        }

        public static Dictionary<PeripheralTreeEntry, IEnumerable<IRegistrationPoint>> GetPeripheralsWithAllRegistrationPoints(this Machine machine)
        {
            var result = new Dictionary<PeripheralTreeEntry, IEnumerable<IRegistrationPoint>>();

            var peripheralEntries = machine.GetRegisteredPeripherals().ToArray();
            foreach(var entryList in peripheralEntries.OrderBy(x => x.Name).GroupBy(x => x.Peripheral))
            {
                var uniqueEntryList = entryList.DistinctBy(x => x.RegistrationPoint).ToArray();
                var entry = uniqueEntryList.FirstOrDefault();
                if(entry != null)
                {
                    result.Add(entry, uniqueEntryList.Select(x => x.RegistrationPoint).ToList());
                }
            }

            return result;
        }
    }
}
