//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals.Bus;
using Emul8.Core;

namespace Emul8.Extensions.Hooks
{
    public static class BusHooksExtensions
    {
        public static void SetHookAfterRead(this SystemBus sysbus, IBusPeripheral peripheral, string pythonScript, Range? subrange = null)
        {
            var runner = new BusHooksPythonEngine(sysbus, peripheral, pythonScript);
            sysbus.SetHookAfterRead<uint>(peripheral, runner.ReadHook, subrange);
            sysbus.SetHookAfterRead<ushort>(peripheral, (readValue, offset) => (ushort)runner.ReadHook(readValue, offset), subrange);
            sysbus.SetHookAfterRead<byte>(peripheral, (readValue, offset) => (byte)runner.ReadHook(readValue, offset), subrange);
        }

        public static void SetHookBeforeWrite(this SystemBus sysbus, IBusPeripheral peripheral, string pythonScript, Range? subrange = null)
        {
            var runner = new BusHooksPythonEngine(sysbus, peripheral, null, pythonScript);
            sysbus.SetHookBeforeWrite<uint>(peripheral, runner.WriteHook, subrange);
            sysbus.SetHookBeforeWrite<ushort>(peripheral, (valueToWrite, offset) => (ushort)runner.WriteHook(valueToWrite, offset), subrange);
            sysbus.SetHookBeforeWrite<byte>(peripheral, (valueToWrite, offset) => (byte)runner.WriteHook(valueToWrite, offset), subrange);
        }
    }
}

