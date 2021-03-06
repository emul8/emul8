//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using System.IO;
using Emul8.Core;
using Emul8.Exceptions;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using Emul8.UserInterface;
using IronPython.Runtime;
using Microsoft.Scripting;

namespace Emul8.Peripherals.Python
{
    public static class PythonPeripheralExtensions
    {
        public static void PyDevFromFile(this Machine @this, string path, long address, int size, bool initable = false, string name = null, long offset = 0)
        {
            var pyDev = new PythonPeripheral(size, initable, filename: path);
            @this.SystemBus.Register(pyDev, new BusPointRegistration(address, offset));
            if(!string.IsNullOrEmpty(name))
            {
                @this.SetLocalName(pyDev, name);
            }
        }

        public static void PyDevFromString(this Machine @this, string script, long address, int size, bool initable = false, string name = null, long offset = 0)
        {
            var pyDev = new PythonPeripheral(size, initable, script: script);
            @this.SystemBus.Register(pyDev, new BusPointRegistration(address, offset));
            if(!string.IsNullOrEmpty(name))
            {
                @this.SetLocalName(pyDev, name);
            }
        }
    }

    [Icon("python")]
    public class PythonPeripheral : IBytePeripheral, IWordPeripheral, IDoubleWordPeripheral, IKnownSize, IAbsoluteAddressAware
    {
        public PythonPeripheral(int size, bool initable = false, string script = null, string filename = null) 
        {
            this.size = size;
            this.initable = initable;
            this.script = script;
            this.filename = filename;

            if((this.script == null && this.filename == null) || (this.script != null && this.filename != null))
            {
                throw new ConstructionException("Parameters `script` and `filename` cannot be both set or both unset.");
            }
            if(this.script != null)
            {
                this.pythonRunner = new PeripheralPythonEngine(this, x => x.CreateScriptSourceFromString(this.script));
            }
            else if(this.filename != null)
            {
                if(!File.Exists(this.filename))
                {
                    throw new ConstructionException(string.Format("Could not find source file for the script: {0}.", this.filename));
                }
                this.pythonRunner = new PeripheralPythonEngine(this, x => x.CreateScriptSourceFromFile(this.filename));
            }
        }

        public void SetAbsoluteAddress(long address)
        {
            pythonRunner.Request.absolute = address;
        }

        public byte ReadByte(long offset)
        {
            pythonRunner.Request.length = 1;
            HandleRead(offset);
            return unchecked((byte)pythonRunner.Request.value);
        }

        public void WriteByte(long offset, byte value)
        {
            pythonRunner.Request.length = 1;
            HandleWrite(offset, value);
        }

        public uint ReadDoubleWord(long offset)
        {
            pythonRunner.Request.length = 4;
            HandleRead(offset);
            return unchecked(pythonRunner.Request.value);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            pythonRunner.Request.length = 4;
            HandleWrite(offset, value);
        }

        public ushort ReadWord(long offset)
        {
            pythonRunner.Request.length = 2;
            HandleRead(offset);
            return unchecked((ushort)pythonRunner.Request.value);
        }

        public void WriteWord(long offset, ushort value)
        {
            pythonRunner.Request.length = 2;
            HandleWrite(offset, value);
        }

        public void Reset()
        {
            inited = false;
            EnsureInit();
        }

        public long Size
        {
            get { return size; }
        }

        public void EnsureInit()
        {
            if(!inited)
            {
                Machine mach;
                if(EmulationManager.Instance.CurrentEmulation.TryGetMachineForPeripheral(this, out mach))
                {
                    pythonRunner.SetSysbusAndMachine(mach.SystemBus);
                }

                Init();
                inited = true;
            }
        }

        public string Code
        {
            get
            {
                return pythonRunner.Code;
            }
        }

        private void Init()
        {
            if(initable)
            {
                pythonRunner.Request.type = PeripheralPythonEngine.PythonRequest.RequestType.INIT;
                Execute();
            }
        }

        private void HandleRead(long offset)
        {
            EnsureInit();

            pythonRunner.Request.value = 0;
            pythonRunner.Request.type = PeripheralPythonEngine.PythonRequest.RequestType.READ;
            pythonRunner.Request.offset = offset;
            Execute();
        }

        private void HandleWrite(long offset, uint value)
        {
            EnsureInit();

            pythonRunner.Request.value = value;
            pythonRunner.Request.type = PeripheralPythonEngine.PythonRequest.RequestType.WRITE;
            pythonRunner.Request.offset = offset;
            Execute();
        }

        private void Execute()
        {
            try
            {
                pythonRunner.ExecuteCode();
            }
            catch(Exception e)
            {
                if(e is SyntaxErrorException || e is UnboundNameException || e is MissingMemberException)
                {
                    this.Log(LogLevel.Error, "Python peripheral error: " + e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        private bool inited;

        private readonly PeripheralPythonEngine pythonRunner;
        private readonly bool initable;
        private readonly int size;
        private readonly string script;
        private readonly string filename;
    }
}
