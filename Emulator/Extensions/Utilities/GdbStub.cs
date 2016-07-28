//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Logging;
using Emul8.Peripherals.CPU;
using Emul8.Utilities.GDB;
using Emul8.Utilities.GDB.Commands;

namespace Emul8.Utilities
{
    public class GdbStub : IDisposable, IExternal
    {
        public GdbStub(int port, ICpuSupportingGdb cpu)
        {
            this.cpu = cpu;
            Port = port;

            pcktBuilder = new PacketBuilder();

            commands = new CommandsManager(cpu);
            TypeManager.Instance.AutoLoadedType += commands.Register;

            cpu.Halted += OnHalted;
            cpu.ExecutionMode = ExecutionMode.SingleStep;

            terminal = new SocketServerProvider();
            terminal.DataReceived += OnByteWritten;
            terminal.Start(port);
        }

        public void Dispose()
        {
            cpu.Halted -= OnHalted;
            terminal.Dispose();
        }

        public int Port { get; private set; }

        private void OnHalted(HaltArguments args)
        {
            switch(args.Reason)
            {
            case HaltReason.Breakpoint:
                switch(args.BreakpointType)
                {
                case BreakpointType.AccessWatchpoint:
                case BreakpointType.WriteWatchpoint:
                case BreakpointType.ReadWatchpoint:
                    beforeCommand += cmd =>
                    {
                        commandsCounter++;
                        if(commandsCounter > 15)
                        {
                            // this is a hack!
                            // I noticed that GDB will send `step` command after receiving
                            // information about watchpoint being hit.
                            // As a result cpu would execute next instruction and stop again.
                            // To prevent this situation we wait for `step` and ignore it, but
                            // only in small time window (15 - instructions, value choosen at random)
                            // and only after sending watchpoint-related stop reply.
                            this.Log(LogLevel.Error, "Expected step command after watchpoint. Further debugging might not work properly");
                            beforeCommand = null;
                            commandsCounter = 0;
                            return false;
                        }
                        if((cmd is SingleStepCommand))
                        {
                            SendPacket(new Packet(PacketData.StopReply(TrapSignal)));
                            beforeCommand = null;
                            commandsCounter = 0;
                            return true;
                        }
                        return false;
                    };
                    goto case BreakpointType.HardwareBreakpoint;
                case BreakpointType.HardwareBreakpoint:
                case BreakpointType.MemoryBreakpoint:
                    SendPacket(new Packet(PacketData.StopReply(args.BreakpointType.Value, args.Address)));
                    break;
                }
                return;
            case HaltReason.Step:
            case HaltReason.Pause:
                SendPacket(new Packet(PacketData.StopReply(TrapSignal)));
                return;
            case HaltReason.Abort:
                SendPacket(new Packet(PacketData.AbortReply(AbortSignal)));
                return;
            default:
                throw new ArgumentException("Unexpected halt reason");
            }
        }

        private void OnByteWritten(byte b)
        {
            var result = pcktBuilder.AppendByte(b);
            if(result == null)
            {
                return;
            }

            if(result.Interrupt)
            {
                cpu.Log(LogLevel.Debug, "GDB CTRL-C occured - pausing CPU");
                // we need to pause CPU in order to escape infinite loops
                cpu.Pause();
                cpu.ExecutionMode = ExecutionMode.SingleStep;
                cpu.Resume();
                return;
            }
            if(result.CorruptedPacket)
            {
                cpu.Log(LogLevel.Warning, "Corrupted GDB packet received: {0}", result.Packet.Data.DataAsString);
                // send NACK
                terminal.SendByte((byte)'-');
                return;
            }

            cpu.Log(LogLevel.Debug, "GDB packet received: {0}", result.Packet.Data.DataAsString);
            // send ACK
            terminal.SendByte((byte)'+');

            Command command;
            if(!commands.TryGetCommand(result.Packet, out command))
            {
                cpu.Log(LogLevel.Warning, "Unsupported GDB command: {0}", result.Packet.Data.DataAsString);
                SendPacket(new Packet(PacketData.Empty));
            }
            else
            {
                var before = beforeCommand;
                if(before != null && before(command))
                {
                    return;
                }
                var packetData = Command.Execute(command, result.Packet);
                // null means that we will response later with Stop Reply Response
                if(packetData != null)
                {
                    SendPacket(new Packet(packetData));
                }
            }
        }

        private void SendPacket(Packet packet)
        {
            cpu.Log(LogLevel.Debug, "Sending response to GDB: {0}", packet.Data.DataAsString);
            foreach(var b in packet.GetCompletePacket())
            {
                terminal.SendByte(b);
            }
        }

        private int commandsCounter;
        private Func<Command, bool> beforeCommand;

        private readonly PacketBuilder pcktBuilder;
        private readonly ICpuSupportingGdb cpu;
        private readonly SocketServerProvider terminal;
        private readonly CommandsManager commands;

        private const int TrapSignal = 5;
        private const int AbortSignal = 6;
    }
}

