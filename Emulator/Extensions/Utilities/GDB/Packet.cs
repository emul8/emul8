//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Text;

namespace Emul8.Utilities.GDB
{
    internal class Packet
    {
        public static bool TryCreate(byte[] data, byte checksum, out Packet p)
        {
            p = new Packet(new PacketData(data));
            return p.CalculateChecksum() == checksum;
        }

        public Packet(PacketData data)
        {
            Data = data;
        }

        public byte[] GetCompletePacket()
        {
            return Encoding.ASCII.GetBytes(string.Format("${0}#{1:x2}", Data.DataAsString, CalculateChecksum()));
        }

        public byte CalculateChecksum()
        {
            uint result = 0;
            foreach(var b in Data.DataAsBinary)
            {
                result += b;
            }
            return (byte)(result % 256);
        }

        public PacketData Data { get; private set; }
    }
}

