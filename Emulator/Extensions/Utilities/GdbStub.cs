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
        public static void StartGDBServer(this IControllableCPU cpu, [AutoParameter] Machine machine, int port)
        {
            var stub = GdbStub.CreateAndListenOnPort(port, cpu, machine);
            EmulationManager.Instance.CurrentEmulation.ExternalsManager.AddExternal(stub, "GDBServer");
        }
    }

    public class GdbStub : IDisposable, IExternal
    {
        public static GdbStub CreateAndListenOnPort(int port, IControllableCPU cpu, Machine machine)
        {
            lock(gdbs)
            {
                if(gdbs.ContainsKey(cpu))
                {
                    throw new RecoverableException(string.Format("GDB server already started for this cpu on port: {0}", gdbs[cpu].Port));
                }

                try
                {
                    var stub = new GdbStub(port, cpu, machine);
                    gdbs.Add(cpu, stub);
                    return stub;
                }
                catch(SocketException e)
                {
                    throw new RecoverableException(string.Format("Could not start GDB server: {0}", e.Message));
                }
            }
        }

        public void Dispose()
        {
            cpu.Halted -= OnHalted;
            terminal.Dispose();
        }

        public int Port { get; private set; }

        private GdbStub(int port, IControllableCPU cpu, Machine machine)
        {
            this.cpu = cpu;
            Port = port;

            terminal = new SocketServerProvider();
            terminal.DataReceived += OnByteWritten;
            terminal.Start(port);

            pcktBuilder = new PacketBuilder();

            commands = new CommandsManager();
            commands.Register(new ReadMemoryCommand(machine));
            commands.Register(new SupportedQueryCommand());
            commands.Register(new ReportHaltReasonCommand());
            commands.Register(new ReadGeneralRegistersCommand(cpu));
            commands.Register(new ContinueCommand((TranslationCPU)cpu));
            commands.Register(new WriteDataToMemoryCommand(machine));
            commands.Register(new WriteBinaryDataToMemoryCommand(machine));
            commands.Register(new WriteRegisterCommand(cpu));
            commands.Register(new ReadRegisterCommand(cpu));
            commands.Register(new SingleStepCommand((TranslationCPU)cpu));
            var watchpointsContext = new WatchpointsContext(cpu);
            commands.Register(new InsertBreakpointCommand(cpu, machine.SystemBus, watchpointsContext));
            commands.Register(new RemoveBreakpointCommand(cpu, machine.SystemBus, watchpointsContext));
            commands.Register(new KillCommand());

            cpu.Halted += OnHalted;
            cpu.ExecutionMode = ExecutionMode.SingleStep;
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
                cpu.ExecutionMode = ExecutionMode.SingleStep;
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

            PacketData packetData = null;
            Command command;
            if(!commands.TryGetCommand(result.Packet.Data.DataAsString, out command))
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
                packetData = command.Handle(result.Packet);
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

        private static readonly Dictionary<IControllableCPU, GdbStub> gdbs = new Dictionary<IControllableCPU, GdbStub>();

        private int commandsCounter;
        private Func<Command, bool> beforeCommand;

        private readonly PacketBuilder pcktBuilder;
        private readonly IControllableCPU cpu;
        private readonly SocketServerProvider terminal;
        private readonly CommandsManager commands;

        private const int TrapSignal = 5;
        private const int AbortSignal = 6;
    }
}

