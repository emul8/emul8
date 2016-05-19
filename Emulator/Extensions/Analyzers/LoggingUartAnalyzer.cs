//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using Emul8.Logging;
using Emul8.Peripherals.UART;
using Emul8.Utilities;
using Antmicro.Migrant;
using Emul8.Core;
using Emul8.Exceptions;
using Emul8.Peripherals;

namespace Emul8.Analyzers
{
    // This class is marked as `IExternal` to allow access to `LogLevel`
    // property from monitor. In order to do this one must create analyzer
    // and add it as external using command below:
    //
    // showAnalyzer "ExternalName" sysbus.uart_name "LoggingAnalyzer"
    //
    [Transient]
    public class LoggingUartAnalyzer : BasicPeripheralBackendAnalyzer<UARTBackend>, IExternal
    {
        public LoggingUartAnalyzer()
        {
            line = new StringBuilder(InitialCapacity);
            LogLevel = LogLevel.Debug;
        }

        public override void AttachTo(UARTBackend backend)
        {
            base.AttachTo(backend);
            uart = backend.UART;

            // let's find out to which machine this uart belongs
            if(!EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(uart, out machine))
            {
                throw new RecoverableException("Given uart does not belong to any machine.");
            }
        }

        public override void Show()
        {
            lastLineStampHost = CustomDateTime.Now;
            lastLineStampVirtual = machine.ElapsedVirtualTime;

            uart.CharReceived += WriteChar;
        }

        public override void Hide()
        {
            uart.CharReceived -= WriteChar;
        }

        public LogLevel LogLevel { get; set; }

        private void WriteChar(byte value)
        {
            if(value == 10)
            {
                var now = CustomDateTime.Now;
                var virtualNow = machine.ElapsedVirtualTime;
                uart.Log(LogLevel, "[+{0}s host +{1}s virt {2}s virt from start] {3}", Misc.NormalizeDecimal((now - lastLineStampHost).TotalSeconds),
                    Misc.NormalizeDecimal((virtualNow - lastLineStampVirtual).TotalSeconds),
                    Misc.NormalizeDecimal(machine.ElapsedHostTime.TotalSeconds),
                    line.ToString());
                lastLineStampHost = now;
                lastLineStampVirtual = virtualNow;
                line.Clear();
                line.Append("  ");
                return;
            }
            if(char.IsControl((char)value))
            {
                return;
            }

            var nextCharacter = (char)value;
            line.Append(nextCharacter);
        }

        private DateTime lastLineStampHost;
        private TimeSpan lastLineStampVirtual;
        private IUART uart;
        private Machine machine;

        private readonly StringBuilder line;

        private const int InitialCapacity = 120;
    }
}

