//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using Emul8.Core;

namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("M")]
    internal class WriteDataToMemoryCommand : WriteDataToMemoryCommandBase
    {
        public WriteDataToMemoryCommand(Machine machine) : base(machine)
        {
        }

        protected override bool TryDecodeData(PacketData packetData, uint length, int location, out byte[] data)
        {
            if(packetData.DataAsString.Length != length * 2 + location)
            {
                data = null;
                return false;
            }

            data = new byte[length];
            for(var i = 0; i < data.Length; i++)
            {
                if(!byte.TryParse(packetData.DataAsString.Substring(location + i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out data[i]))
                {
                    throw new ArgumentException("Could not parse value");
                }
            }
            return true;
        }
    }

    [Mnemonic("X")]
    internal class WriteBinaryDataToMemoryCommand : WriteDataToMemoryCommandBase
    {
        public WriteBinaryDataToMemoryCommand(Machine machine) : base(machine)
        {       
        }

        protected override bool TryDecodeData(PacketData packetData, uint length, int location, out byte[] data)
        {
            if(packetData.DataAsBinary.Length != length + location)
            {
                data = null;
                return false;
            }

            data = packetData.DataAsBinary.Skip(location).ToArray();
            return true;
        }
    }

    internal abstract class WriteDataToMemoryCommandBase : Command
    {
        protected WriteDataToMemoryCommandBase(Machine machine)
        {
            this.machine = machine;
        }

        protected override PacketData HandleInner(Packet packet)
        {
            var arguments = GetCommandArguments(packet.Data, ArgumentsSplitCharacters, 3);
            if(arguments.Length < 2)
            {
                throw new ArgumentException("Expected at least two arguments");
            }

            uint address;
            if(!uint.TryParse(arguments[0], System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                throw new ArgumentException("Could not parse address");
            }

            uint size;
            if(!uint.TryParse(arguments[1], System.Globalization.NumberStyles.HexNumber, null, out size))
            {
                throw new ArgumentException("Could not parse size");
            }

            byte[] data;
            // we must sum lengths of mnemonic, two arguments and split characters
            if(!TryDecodeData(packet.Data, size, arguments[0].Length + arguments[1].Length + 3, out data))
            {
                throw new ArgumentException("Could not decode data");
            }

            machine.SystemBus.WriteBytes(data, address);
            return PacketData.Success;
        }

        protected abstract bool TryDecodeData(PacketData packetData, uint length, int location, out byte[] data);

        private readonly Machine machine;
        private static readonly char[] ArgumentsSplitCharacters = { ',', ':' };
    }
}

