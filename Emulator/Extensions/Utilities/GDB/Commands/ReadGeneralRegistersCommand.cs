//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Text;
using ELFSharp.ELF;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    internal class ReadGeneralRegistersCommand : Command
    {
        public ReadGeneralRegistersCommand(IControllableCPU cpu) : base("g")
        {
            this.cpu = cpu;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var registers = new StringBuilder();
            foreach(var i in cpu.GetRegisters())
            {
                var value = cpu.GetRegisterUnsafe(i);
                if(cpu.Endianness == Endianess.LittleEndian)
                {
                    value = Helpers.SwapBytes(value);
                }

                registers.AppendFormat("{0:x8}", value);
            }

            return new PacketData(registers.ToString());
        }

        private readonly IControllableCPU cpu;
    }
}

