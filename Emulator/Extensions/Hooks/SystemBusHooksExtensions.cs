//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.Bus;
using Emul8.Core;

namespace Emul8.Hooks
{
    public static class SystemBusHooksExtensions
    {
        public static void SetHookAfterPeripheralRead(this SystemBus sysbus, IBusPeripheral peripheral, string pythonScript, Range? subrange = null)
        {
            var runner = new BusPeripheralsHooksPythonEngine(sysbus, peripheral, pythonScript);
            sysbus.SetHookAfterPeripheralRead<uint>(peripheral, runner.ReadHook, subrange);
            sysbus.SetHookAfterPeripheralRead<ushort>(peripheral, (readValue, offset) => (ushort)runner.ReadHook(readValue, offset), subrange);
            sysbus.SetHookAfterPeripheralRead<byte>(peripheral, (readValue, offset) => (byte)runner.ReadHook(readValue, offset), subrange);
        }

        public static void SetHookBeforePeripheralWrite(this SystemBus sysbus, IBusPeripheral peripheral, string pythonScript, Range? subrange = null)
        {
            var runner = new BusPeripheralsHooksPythonEngine(sysbus, peripheral, null, pythonScript);
            sysbus.SetHookBeforePeripheralWrite<uint>(peripheral, runner.WriteHook, subrange);
            sysbus.SetHookBeforePeripheralWrite<ushort>(peripheral, (valueToWrite, offset) => (ushort)runner.WriteHook(valueToWrite, offset), subrange);
            sysbus.SetHookBeforePeripheralWrite<byte>(peripheral, (valueToWrite, offset) => (byte)runner.WriteHook(valueToWrite, offset), subrange);
        }

        public static void AddWatchpointHook(this SystemBus sysbus, long address, Width width, Access access, string pythonScript)
        {
            var engine = new WatchpointHookPythonEngine(sysbus, pythonScript);
            sysbus.AddWatchpointHook(address, width, access, true, engine.Hook);
        }
    }
}

