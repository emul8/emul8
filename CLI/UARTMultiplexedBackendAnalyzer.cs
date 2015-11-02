//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;
using Emul8.Peripherals.UART;
using Emul8.Backends.Terminals;
using AntShell.Terminal;
using Emul8.Core;

namespace Emul8.CLI 
{
    public class UARTMultiplexedBackendAnalyzer : IAnalyzableBackendAnalyzer<UARTBackend>
    {
        public void AttachTo(UARTBackend backend)
        {
            backend.BindAnalyzer(IO);
            Backend = backend;
        }

        public void Show()
        {
            Machine mach;
            string name;

            var uart = Backend.AnalyzableElement as IUART;
            EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(uart, out mach);
            mach.TryGetLocalName(uart, out name);

            Multiplexer.AttachTerminal(name, IO);
        }

        public void Hide()
        {
            Machine mach;
            string name;

            var uart = Backend.AnalyzableElement as IUART;
            EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(uart, out mach);
            mach.TryGetLocalName(uart, out name);

            Multiplexer.DetachTerminal(name);
        }

        public string Id { get { return "multiplexed-UART"; } }

        public IAnalyzableBackend Backend { get; private set; }

        public UARTMultiplexedBackendAnalyzer()
        {
            IO = new DetachableIO();
        }

        private DetachableIO IO;

        public static ConsoleTerminal Multiplexer { get; set; }
    }
}

