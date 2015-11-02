//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Xml.Linq;
using System.Linq;
using Emul8.Logging;
using System.Collections.Generic;
using Emul8.Exceptions;
using System.Xml;
using System.IO;
using Emul8.Utilities;
using System.IO.Compression;

namespace Emul8.Peripherals.Bus
{
    public sealed class SVDDummyDevice
    {
        public SVDDummyDevice(string path, SystemBus parent)
        {
            this.parent = parent;
            readRegisters = new Dictionary<long, Register>();
            writeRegisters = new Dictionary<long, Register>();
            Stream possibleGzip;
            using(possibleGzip = File.OpenRead(path))
            {
                if(possibleGzip.ReadByte() == 0x1F && possibleGzip.ReadByte() == 0x8B) // gzip header
                {
                    parent.Log(LogLevel.Info, "Detected gzipped file, ungzipping.");
                    possibleGzip.Close();
                    possibleGzip = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
                    path = TemporaryFilesManager.Instance.GetTemporaryFile();
                    using(var extractedFile = File.OpenWrite(path))
                    {
                        possibleGzip.CopyTo(extractedFile);
                    }
                    parent.Log(LogLevel.Info, "Successfully ungzipped.");
                }
            }
            try
            {
                var document = XDocument.Load(path);
                var deviceNode = document.Elements().First(x => x.Name == "device");
                FillRegisters(deviceNode);
                parent.Log(LogLevel.Info, "Loaded SVD: {0}", deviceNode.Descendants().First(x => x.Name == "description").Value);
            }
            catch(XmlException e)
            {
                throw new RecoverableException(string.Format("Given SVD file could not be processed due to an exception: {0}.", e.Message));
            }
        }

        public bool ReadAccess(long offset, out uint value)
        {
            Register register;
            var weHaveIt = readRegisters.TryGetValue(offset, out register);
            value = register.Value;
            if(weHaveIt)
            {
                parent.DebugLog("Read at {0}:{1} (0x{3:X}), 0x{2:X} returned.", register.PeripheralName, register.RegisterName, value, offset);
                return true;
            }
            if(weHaveIt = writeRegisters.TryGetValue(offset, out register))
            {
                parent.Log(LogLevel.Warning, "Read at {0}:{1} (0x{2:X}) while only write access is allowed.", register.PeripheralName,
                           register.RegisterName, offset);
            }
            return weHaveIt;
        }

        public bool WriteAccess(long offset, uint value, string type)
        {
            Register register;
            var weHaveIt = writeRegisters.TryGetValue(offset, out register);
            if(weHaveIt)
            {
                parent.DebugLog("Write value 0x{3:X} ({4}) at {0}:{1} (0x{2:X}).", register.PeripheralName, register.RegisterName, offset, value, type);
                return true;
            }
            if(weHaveIt = readRegisters.TryGetValue(offset, out register))
            {
                parent.Log(LogLevel.Warning, "Write value 0x{3:X} ({4}) at {0}:{1} (0x{2:X}) while only read access is allowed.", register.PeripheralName,
                    register.RegisterName, offset, value, type);
            }
            return weHaveIt;
        }

        private void FillRegisters(XElement deviceNode)
        {
            var peripherals = deviceNode.Element("peripherals").Elements();
            foreach(var peripheral in peripherals)
            {
                ScanPeripheral(peripheral);
            }
        }

        private void ScanPeripheral(XElement peripheralNode, string peripheralName = null, long? baseAddress = null)
        {
            if(baseAddress == null)
            {
                baseAddress = ParseHexOrDecimal(peripheralNode.Element("baseAddress").Value);
            }
            if(peripheralName == null)
            {
                peripheralName = peripheralNode.Element("name").Value;
            }
            var derivedFrom = peripheralNode.Attribute("derivedFrom");
            if(derivedFrom != null)
            {
                ScanPeripheral(peripheralNode.Parent.Elements().First(x => x.Name == "peripheral" && x.Element("name").Value == derivedFrom.Value),
                               peripheralName, baseAddress);
                return;
            }
            var registers = peripheralNode.Element("registers").Elements();
            foreach(var register in registers)
            {
                AddRegister(register, baseAddress.Value, peripheralName);
            }
        }

        private void AddRegister(XElement register, long baseAddress, string peripheralName)
        {
            var registerAddress = baseAddress + ParseHexOrDecimal(register.Element("addressOffset").Value);
            var defaultValue = (uint)ParseHexOrDecimal(register.Element("resetValue").Value);
            var name = register.Element("name").Value;
            PermittedAccess access;
            string accessAsString;
            var accessElement = register.Element("access");
            if(accessElement != null)
            {
                accessAsString = accessElement.Value;
            }
            else
            {
                accessAsString = "read-write";
            }
            switch(accessAsString)
            {
            case "read-only":
                access = PermittedAccess.Read;
                break;
            case "write-only":
                access = PermittedAccess.Write;
                break;
            case "read-write":
                access = PermittedAccess.Read | PermittedAccess.Write;
                break;
            default:
                throw new RecoverableException(string.Format("Found element with unexpected access type: {0}", accessAsString));
            }
            var width = ParseHexOrDecimal(register.Element("size").Value);
            if(width != 32 && width != 16 && width != 8)
            {
                throw new RecoverableException(string.Format("Found element with unexpected register width: {0}", width));
            }
            var registerToAdd = new Register {
                PeripheralName = peripheralName,
                RegisterName = name,
                Value = defaultValue,
                Width = checked((int)width)
            };
            if((access & PermittedAccess.Read) != 0)
            {
                AddRegisterToDictionary(registerAddress, registerToAdd, register, readRegisters);
            }
            if((access & PermittedAccess.Write) != 0)
            {
                AddRegisterToDictionary(registerAddress, registerToAdd, register, writeRegisters);
            }
            parent.NoisyLog("{2}: register {0} at 0x{1:X}, access {3}, default value: 0x{4:X}", name, registerAddress, peripheralName, access, defaultValue);
        }

        private long AddRegisterToDictionary(long registerAddress, Register registerToAdd, XElement registerElement, Dictionary<long, Register> dictionary)
        {
            if(dictionary.ContainsKey(registerAddress))
            {
                dictionary[registerAddress] = readRegisters[registerAddress].Join(registerToAdd);
            }
            else
            {
                dictionary.Add(registerAddress, registerToAdd);
            }
            return registerAddress;
        }

        private static long ParseHexOrDecimal(string whatToParse)
        {
            return Convert.ToInt64(whatToParse, whatToParse.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? 16 : 10);
        }

        private readonly Dictionary<long, Register> readRegisters;
        private readonly Dictionary<long, Register> writeRegisters;
        private readonly SystemBus parent;

        private struct Register
        {
            public string PeripheralName;
            public string RegisterName;
            public uint Value;
            public int Width;

            public Register Join(Register register)
            {
                var result = this;
                result.RegisterName += string.Format("/{0}", register.RegisterName);
                return result;
            }
        }

        [Flags]
        private enum PermittedAccess
        {
            Read = 1,
            Write = 2
        }
    }
}

