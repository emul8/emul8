//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB.Commands
{
    internal class SingleStepCommand : Command
    {
        public SingleStepCommand(TranslationCPU cpu) : base("s")
        {
            this.cpu = cpu;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var address = GetCommandArguments(packet.Data, new[] { ' ' });
            if(address.Length > 0 && !string.IsNullOrEmpty(address[0]))
            {
                throw new InvalidOperationException("Stepping to address is not supported yet.");
            }

            cpu.SetSingleStepMode(true);
            cpu.Step();
            return null;
        }

        private readonly TranslationCPU cpu;
    }
}

