//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Text;
using Emul8.Peripherals.CPU;

namespace Emul8.Utilities.GDB
{
    internal class PacketData
    {
        static PacketData()
        {
            Success = new PacketData("OK");
            Empty = new PacketData(string.Empty);
        }

        public static PacketData AbortReply(int signal)
        {
            return new PacketData(string.Format("X{0:X2}", signal));
        }

        public static PacketData StopReply(int signal)
        {
            return new PacketData(string.Format("S{0:X2}", signal));
        }

        public static PacketData StopReply(BreakpointType reason, long address)
        {
            return new PacketData(string.Format("T05{0}:{1:X2};", reason.GetStopReason(), address));
        }

        public PacketData(string data)
        {
            var encoder = new UTF8Encoding();
            DataAsString = data;
            DataAsBinary = encoder.GetBytes(data);
        }

        public PacketData(byte[] data)
        {
            var encoder = new UTF8Encoding();
            DataAsBinary = data;
            DataAsString = encoder.GetString(data);
        }

        public static PacketData Success { get; private set; }
        public static PacketData Empty { get; private set; }

        public byte[] DataAsBinary { get; private set; }
        public string DataAsString { get; private set; }
    }
}

