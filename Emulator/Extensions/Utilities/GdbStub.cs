//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Logging;
using System.Collections.Generic;
using Emul8.Peripherals.CPU;
using System.Net.Sockets;
using Emul8.Exceptions;
using Emul8.Utilities.GDB;
using Emul8.Utilities.GDB.Commands;

namespace Emul8.Utilities
{
    public static class GdbExtensions
    {
        public static void StartGDBServer(this ICpuSupportingGdb cpu, [AutoParameter] Machine machine, int port)
        {
            string cpuName;
            machine.TryGetLocalName(cpu, out cpuName);
            var stub = GdbStub.CreateAndListenOnPort(port, cpu);
            EmulationManager.Instance.CurrentEmulation.ExternalsManager.AddExternal(stub, string.Format("GdbStub-{0}", cpuName));
        }

        public static void StopGdbServer(this ICpuSupportingGdb cpu)
        {
            GdbStub.Stop(cpu);
        }
    }

    public class GdbStub : IDisposable, IExternal
    {
        public static GdbStub CreateAndListenOnPort(int port, ICpuSupportingGdb cpu)
        {
            lock(activeGdbStubs)
            {
                if(activeGdbStubs.ContainsKey(cpu))
                {
                    throw new RecoverableException(string.Format("GDB server already started for this cpu on port: {0}", activeGdbStubs[cpu].Port));
                }

                try
                {
                    var stub = new GdbStub(port, cpu);
                    activeGdbStubs.Add(cpu, stub);
                    return stub;
                }
                catch(SocketException e)
                {
                    throw new RecoverableException(string.Format("Could not start GDB server: {0}", e.Message));
                }
            }
        }

        public static void Stop(ICpuSupportingGdb cpu)
        {
            GdbStub stub;
            if(activeGdbStubs.TryGetValue(cpu, out stub))
            {
                activeGdbStubs.Remove(cpu);
                stub.Dispose();
            }
        }

        public void Dispose()
        {
            cpu.Halted -= OnHalted;
            terminal.Dispose();
        }

        public int Port { get; private set; }

        private GdbStub(int port, ICpuSupportingGdb cpu)
        {
            this.cpu = cpu;
            Port = port;

            pcktBuilder = new PacketBuilder();

            commands = new CommandsManager(cpu);
            TypeManager.Instance.AutoLoadedType += t => commands.Register(t);

            cpu.Halted += OnHalted;
            cpu.ExecutionMode = ExecutionMode.SingleStep;

            terminal = new SocketServerProvider();
            terminal.DataReceived += OnByteWritten;
            terminal.Start(port);
        }

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
                            throw new RecoverableException("Expected step command after watchpoint. Further debugging might not work properly");
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

        private static readonly Dictionary<ICpuSupportingGdb, GdbStub> activeGdbStubs = new Dictionary<ICpuSupportingGdb, GdbStub>();

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

