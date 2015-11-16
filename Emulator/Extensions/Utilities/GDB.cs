//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
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
using System.Text;

namespace Emul8.Utilities
{
    public static class GDBExtensions
    {
        public static void StartGDBServer(this IControllableCPU cpu, [AutoParameter] Machine machine, int port)
        {
            GDB.CreateAndListenOnPort(port, cpu, machine);
        }
    }

    public class GDB : IDisposable
    {
        private static readonly Dictionary<IControllableCPU, GDB> gdbs = new Dictionary<IControllableCPU, GDB>();

        public static void CreateAndListenOnPort(int port, IControllableCPU cpu, Machine machine)
        {
            if(gdbs.ContainsKey(cpu))
            {
                throw new RecoverableException(string.Format("GDB server already started for this cpu on port: {0}", gdbs[cpu].Port));
            }

            try
            {
                gdbs.Add(cpu, new GDB(port, cpu, machine));
            }
            catch(SocketException e)
            {
                throw new RecoverableException(string.Format("Could not start GDB server: {0}", e.Message));
            }
        }

        public GDB(int port, IControllableCPU cpu, Machine machine)
        {
            this.machine = machine;
            this.cpu = cpu;
            line_l = new List<byte>();
            this.mode = 0;
            this.terminal = new SocketServerProvider();
            terminal.DataReceived += OnByteWritten;
            terminal.Start(port);
            cpu.Halted += OnHalted;
            Port = port;
        }

        void OnHalted(HaltReason reason)
        {
            if((reason == HaltReason.Breakpoint) || (reason == HaltReason.StepMode) || (reason == HaltReason.Abort) || (reason == HaltReason.Pause))
            {
                send_command("S05");
            }
        }

        private void OnByteWritten(byte b)
        {
            if(b == '$')
            {
                this.mode = 1;
            } 
            if(this.mode >= 1)
            {
                line_l.Add(b);
            }
            if(this.mode >= 2)
            {
                this.mode += 1;
            }
            if(b == '#')
            {
                this.mode = 2;
            } 
            if(this.mode == 4)
            { // got CRC after '#'
                this.mode = 0;
                interpret(line_l.ToArray());
                line_l.Clear();
            }
            if((this.mode == 0) && (b == 0x03))
            {
                this.cpu.DebugLog("GDB CTRL-C occured - pausing machine\n");
                this.machine.Pause();
            }
        }

        uint count_crc(byte[] data)
        {
            int i;
            uint crc_c = 0;
            for(i = 1; ((i < data.Length) && (data[i] != '#')); i++)
            {
                crc_c += data[i];
            }
            return crc_c % 256;
        }

        bool check_crc(byte[] ln)
        {
            string crc_s = string.Format("{0:c}{1:c}", (char)(ln[ln.Length - 2]), (char)(ln[ln.Length - 1]));
            uint crc = Convert.ToUInt32(crc_s, 16);
            return (crc == count_crc(ln)); 
        }

        void interpret(byte[] lin)
        {
            this.cpu.DebugLog("Line: {0}, currently at PC=0x{1:X}", lin, cpu.PC);
            if(!check_crc(lin))
            {
                // WRONG CRC
                this.cpu.DebugLog("Line: {0}", lin);
                this.cpu.DebugLog("WRONG CRC!!!");
                terminal.SendByte((byte)'-'); // send '-' (WRONG CRC)
                return;
            }
            terminal.SendByte((byte)'+'); // send '+' (ACK)
            string cmd = System.Text.Encoding.ASCII.GetString(lin).Split('#')[0].Split('$')[1].Split(':')[0]; // TODO: this is shitty, but should work here.
            if(cmd[0] != 'X')
            {
                this.cpu.DebugLog("cmd is {0}", cmd);
            }
            string cmd_data = cmd.Substring(1);

            if(cmd[0] == 'X')
            {
                // WRITE BYTES
                uint addr = Convert.ToUInt32(cmd_data.Split(',')[0], 16);
                uint size = Convert.ToUInt32(cmd_data.Split(',')[1], 16);
                if(size > 0)
                {
                    this.cpu.DebugLog("We have {0} data to write @ {1:X}.", size, addr);
                    int i = 0;
                    int delta = cmd.Length + 1 + 1;// cmd len + "$" + ":"
                    int written = 0;
                    bool escape = false;
                    byte[] buf = new byte[size];
                    while(written < size)
                    {
                        if(escape)
                        {
                            buf[written] = (byte)(lin[delta + i] ^ 0x20);
                            //machine.SystemBus.WriteByte(addr + written, (byte)(lin[delta + i] ^ 0x20));
                            escape = false;
                            written++;
                        }
                        else if(lin[delta + i] != 0x7D)
                        {
                            buf[written] = lin[delta + i];
                            //machine.SystemBus.WriteByte(addr + written, lin[delta + i]);
                            written++;
                        }
                        else
                        {
                            escape = true;
                        }
                        i++;
                    }
                    /*  for (i = 0; i < size / 4; i+=4) {
                        uint val = BitConverter.ToUInt32(buf,i);
                        machine.SystemBus.WriteDoubleWord(addr+i, swapEndianness(val));
                    }*/
                    machine.SystemBus.WriteBytes(buf, addr);
                }
                send_command("OK");
            }
            else if(cmd[0] == 'q')
            {
                //Console.WriteLine("query : {0}",cmd_data);
                // QUERY
                string query_cmd = cmd_data.Split(',')[0];
                this.cpu.DebugLog("query_cmd: {0}", query_cmd);
                if(query_cmd == "Supported")
                {
                    send_command(string.Format("PacketSize={0:x4}", 4096));
                }
                else if(query_cmd == "Rcmd")
                {
                    if(this.OnMonitor == null)
                    {
                        send_command(""); // not supported
                    }
                    else
                    {
                        // mon
                        string str = cmd_data.Split(',')[1];
                        CharEnumerator charEnum = str.GetEnumerator();
                        var string_builder = new System.Text.StringBuilder();
                        while(charEnum.MoveNext())
                        {
                            string hex = string.Format("{0}", charEnum.Current);
                            charEnum.MoveNext();
                            hex = hex + string.Format("{0}", charEnum.Current);
                            byte val = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                            string_builder.Append((char)val);
                        }
                        string mon_cmd = string_builder.ToString();
                        this.cpu.DebugLog("mon_cmd = {0}", mon_cmd);
                        this.OnMonitor(mon_cmd);
                        send_command("OK");
                    }
                }
                else
                {
                    send_command(""); // empty
                }
            }
            else if(cmd == "?")
            {
                machine.Pause();
            }
            else if(cmd[0] == 'H')
            {
                if(cmd == "Hc-1")
                { // future step/continue on all threads
                    send_command("OK"); 
                }
                else
                {
                    send_command("");
                }
            }
            else if(cmd == "g")
            {
                this.cpu.DebugLog("read registers...");
                int i;
                string registers = "";
                for(i = 0; i < 16; i++)
                {
                    char[] reg = string.Format("{0:x8}", cpu.GetRegisterUnsafe(i)).ToCharArray();
                    Array.Reverse(reg);
                    int j;
                    for(j = 0; j < 8; j += 2)
                    {
                        registers = registers + string.Format("{0:c}{1:c}", reg[j + 1], reg[j]);
                    }
                }
                this.cpu.DebugLog("sending registers...");
                send_command(registers); // send registers
            }
            else if(cmd[0] == 'm')
            { // read memory
                string[] data = cmd.Split('m')[1].Split(',');
                uint offset = Convert.ToUInt32(data[0], 16);
                uint size = Convert.ToUInt32(data[1], 16);
                //Console.WriteLine("Trying to read @ {0:X} size {1}", offset, size);
                uint result = 0;
                if(size == 4)
                {
                    result = machine.SystemBus.ReadDoubleWord(offset); // TODO: should support diffrent sizes than 4
                    byte[] temp = BitConverter.GetBytes(result);
                    Array.Reverse(temp);
                    result = System.BitConverter.ToUInt32(temp, 0);
                    send_command(string.Format("{0:x8}", result));
                }
                else if(size == 1)
                {
                    result = machine.SystemBus.ReadByte(offset);
                    send_command(string.Format("{0:x2}", result));
                }
                else if(size == 2)
                {
                    result = machine.SystemBus.ReadWord(offset); // TODO: Endian conversion
                    send_command(string.Format("{0:x4}", result));
                }
                else
                {
                    // multibyte read
                    var res = machine.SystemBus.ReadBytes((long)offset, (int)size);
                    string s = "";
                    for(int i = 0; i < size; i++)
                        s = s + string.Format("{0:x2}", res[i]);
                    send_command(s);
                }
            }
            else if(cmd[0] == 'p')
            { // read register ?
                uint reg_no = Convert.ToUInt32(cmd.Split('p')[1]);
                if(reg_no == 19)
                {
                    send_command(string.Format("{0:x8}", cpu.GetRegisterUnsafe(15)));
                }
            }
            else if(cmd[0] == 'P')
            {
                if(cmd_data[0] == 'f')
                {
                    string addr_s = cmd_data.Split('=')[1];
                    uint addr = Convert.ToUInt32(addr_s, 16);
                    byte[] temp = BitConverter.GetBytes(addr);
                    Array.Reverse(temp);
                    addr = System.BitConverter.ToUInt32(temp, 0);
                    this.cpu.DebugLog("Setting PC to {0:X}", addr);
                    cpu.PC = addr;
                    cpu.Reset();
                    cpu.PC = addr;
 
                }
                send_command("OK");
            }
            else if(cmd[0] == 'z')
            {
                string[] data = cmd_data.Split(',');
                cpu.DebugLog("Remove breakpoint_{0} to {1} size {2}", data[0], data[1], data[2]);
                uint addr = Convert.ToUInt32(data[1], 16);
                cpu.RemoveBreakpoint(addr); 
                send_command("OK");
             
            }
            else if(cmd[0] == 'Z')
            { // TODO
                string[] data = cmd_data.Split(',');
                cpu.DebugLog("Set breakpoint_{0} to {1} size {2}", data[0], data[1], data[2]); 
                uint addr = Convert.ToUInt32(data[1], 16);
                cpu.AddBreakpoint(addr);
                send_command("OK");
            }
            else if(cmd[0] == 'v')
            {
                // if 's' : Cont? and then Cont;s
                // if 'c' : Cont? and then Cont;c
                if(cmd_data == "Cont;s")
                {
                    cpu.DebugLog("Single stepping...");
                    if(cpu.InSingleStep())
                    {
                        cpu.DebugLog("already in singlestep, doing another step...");
                        cpu.SingleStep();
                    }
                    else
                    {
                        cpu.SetSingleStepMode(true);
                        machine.Start();
                    }
                    cpu.DebugLog("Waiting for step done");
                }
                else if(cmd_data == "Cont;c")
                {
                    cpu.DebugLog("GDB: We're going to continue @ PC=0x{0:X}", cpu.PC);
                    cpu.SetSingleStepMode(false);
                    machine.Start();
                    cpu.DebugLog("Waiting for breakpoint");
                }
                else
                {
                    send_command("OK");
                }
            }
        }

        public void Dispose()
        {
            cpu.Halted -= OnHalted;
        }

        void send_command(string ln)
        {
            string cmd = "$" + ln;
            var enc = new UTF8Encoding();
            uint crc = count_crc(enc.GetBytes(cmd));
            cmd = cmd + "#" + string.Format("{0:x2}", crc);
            foreach(var b in enc.GetBytes(cmd))
            {
                terminal.SendByte(b);
            }
        }

        public int Port { get; private set; }

        private readonly IControllableCPU cpu;
        private List<byte> line_l;
        private int mode;
        private readonly SocketServerProvider terminal;
        private readonly Machine machine;

        public event Action<string> OnMonitor;
    }
}

