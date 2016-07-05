//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Text;

namespace Emul8.Utilities.GDB
{
    internal class PacketBuilder
    {
        public PacketBuilder()
        {
            buffer = new Queue<byte>();
            checksum = new byte[2];
        }

        public Result AppendByte(byte b)
        {
            switch(state)
            {
            case State.NoPacket:
                switch(b)
                {
                case InterruptSymbol:
                    return new Result(interrupt: true);
                case PacketStartSymbol:
                    state = State.Data;
                    buffer.Clear();
                    break;
                }
                break;
            case State.Data:
                if(escape)
                {
                    b = (byte)(b ^ EscapeOffset);
                    buffer.Enqueue(b);
                    escape = false;
                    break;
                }

                switch(b)
                {
                case EscapeSymbol:
                    escape = true;
                    break;
                case PacketStopSymbol:
                    state = State.Checksum1;
                    break;
                default:
                    buffer.Enqueue(b);
                    break;
                }
                break;
            case State.Checksum1:
                checksum[0] = b;
                state = State.Checksum2;
                break;
            case State.Checksum2:
                checksum[1] = b;
                state = State.NoPacket;

                var checksumValue = (byte)Convert.ToUInt32(Encoding.ASCII.GetString(checksum), 16);
                Packet packet;
                if(!Packet.TryCreate(buffer.ToArray(), checksumValue, out packet))
                {
                    return new Result(packet, corruptedPacket: true);
                }
                return new Result(packet);
            }

            return null;
        }

        private bool escape;
        private State state;

        private readonly Queue<byte> buffer;
        private readonly byte[] checksum;

        private const byte PacketStartSymbol = (byte)'$';
        private const byte PacketStopSymbol = (byte)'#';
        private const byte EscapeSymbol = (byte)'}';
        private const byte InterruptSymbol = 0x03;
        private const byte EscapeOffset = 0x20;

        private enum State
        {
            NoPacket,
            Data,
            Checksum1,
            Checksum2
        }

        internal class Result
        {
            public Result(Packet packet = null, bool corruptedPacket = false, bool interrupt = false)
            {
                Packet = packet;
                Interrupt = interrupt;
                CorruptedPacket = corruptedPacket;
            }

            public Packet Packet { get; private set; }
            public bool Interrupt { get; private set; }
            public bool CorruptedPacket { get; private set; }
        }
    }
}

