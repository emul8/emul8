//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using Emul8.Core;

namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("m")]
    internal class ReadMemoryCommand : Command
    {
        public ReadMemoryCommand(Machine machine)
        {
            this.machine = machine;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var splittedArguments = GetCommandArguments(packet.Data, SplitChars, 2);
            if(splittedArguments.Length != 2)
            {
                throw new ArgumentException("Expected two arguments");
            }

            long address;
            if(!long.TryParse(splittedArguments[0], System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                throw new ArgumentException("Could not parse address");
            }

            int size;
            if(!int.TryParse(splittedArguments[1], System.Globalization.NumberStyles.HexNumber, null, out size))
            {
                throw new ArgumentException("Could not parse size");
            }

            var content = new StringBuilder();

            // ReadBytes returns data in original endianess, thus
            // no processing is needed here
            foreach(var b in machine.SystemBus.ReadBytes(address, size))
            {
                content.AppendFormat("{0:x2}", b);
            }

            return new PacketData(content.ToString());
        }

        private readonly Machine machine;
        private static char[] SplitChars = { ',' };
    }
}

