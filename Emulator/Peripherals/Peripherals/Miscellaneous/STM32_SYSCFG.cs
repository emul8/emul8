//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Emul8.Core;
using Emul8.Core.Structure.Registers;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.IRQControllers;

namespace Emul8.Peripherals.Miscellaneous
{
    public sealed class STM32_SYSCFG : IDoubleWordPeripheral, IIRQController, INumberedGPIOOutput, IKnownSize
    {
        public STM32_SYSCFG()
        {
            var gpios = new Dictionary<int, IGPIO>();
            for(var i = 0; i < GpioPins; ++i)
            {
                gpios.Add(i, new GPIO());
            }
            Connections = new ReadOnlyDictionary<int, IGPIO>(gpios);
            registers = CreateRegisters();
        }

        public void OnGPIO(int number, bool value)
        {
            var pinNumber = number % GpioPins;
            var portNumber = number / GpioPins;
            if(extiMappings[pinNumber].Value == portNumber)
            {
                Connections[pinNumber].Set(value);
            }
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public void Reset()
        {
            foreach(var connection in Connections.Values)
            {
                connection.Unset();
            }
            registers.Reset();
        }

        public IReadOnlyDictionary<int, IGPIO> Connections
        {
            get; private set;
        }

        public long Size
        {
            get
            {
                return 0x400;
            }
        }

        private DoubleWordRegisterCollection CreateRegisters()
        {
            var map = new Dictionary<long, DoubleWordRegister>();
            for(var regNumber = 0; regNumber < 4; ++regNumber)
            {
                var reg = new DoubleWordRegister(this, 0);
                for(var fieldNumber = 0; fieldNumber < 4; ++fieldNumber)
                {
                    var rn = regNumber;
                    var fn = fieldNumber;
                    extiMappings[regNumber * 4 + fieldNumber] = reg.DefineValueField(4 * fieldNumber, 4, name: "EXTI" + regNumber * 4 + fieldNumber, changeCallback: (_, __) => Connections[rn * 4 + fn].Unset());
                }
                map.Add((long)Registers.ExternalInterruptConfiguration1 + 4 * regNumber, reg);
            }
            return new DoubleWordRegisterCollection(this, map);
        }

        private readonly DoubleWordRegisterCollection registers;

        private readonly IValueRegisterField[] extiMappings = new IValueRegisterField[GpioPins];

        private const int GpioPins = 16;

        private enum Registers
        {
            MemoryRemap = 0x0,
            PeripheralModeConfiguration = 0x4,
            ExternalInterruptConfiguration1 = 0x8,
            ExternalInterruptConfiguration2 = 0xC,
            ExternalInterruptConfiguration3 = 0x10,
            ExternalInterruptConfiguration4 = 0x14,
            CompensationCellControl = 0x20
        }
    }
}
