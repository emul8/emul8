//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Core.Structure;
using Emul8.Exceptions;
using Emul8.Logging;
using Emul8.Peripherals.Bus.Wrappers;
using Emul8.Peripherals.CPU;
using Emul8.Utilities;
using Emul8.Peripherals;
using System.Threading;
using System.Collections.ObjectModel;
using System.Text;
using Emul8.Peripherals.Bus;
using Machine = Emul8.Core.Machine;
using Antmicro.Migrant;
using ELFSharp.ELF;
using ELFSharp.ELF.Segments;
using ELFSharp.UImage;
using System.IO;
using Emul8.Core.Extensions;
using System.Reflection;
using Emul8.UserInterface;
using Emul8.Peripherals.Memory;
using ELFSharp.ELF.Sections;
using MiscUtil.Conversion;

namespace Emul8.Peripherals.Bus
{
    /// <summary>
    ///     The <c>SystemBus</c> is the main system class, where all data passes through.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A sample remark,
    ///     </para>
    ///     <para>
    ///         and the second paragraph.
    ///     </para>
    /// </remarks>
    [Icon("sysbus")]
    [ControllerMask(typeof(IPeripheral))]
    public sealed partial class SystemBus : IPeripheralContainer<IBusPeripheral, BusRangeRegistration>, IPeripheralRegister<IKnownSize, BusPointRegistration>,
        IPeripheralRegister<ICPU, CPURegistrationPoint>, IDisposable, IPeripheral, IPeripheralRegister<IBusPeripheral, BusMultiRegistration>
    {
        internal SystemBus(Machine machine)
        {
            this.machine = machine;
            cpuSync = new object();
            binaryFingerprints = new List<BinaryFingerprint>();
            cpuById = new Dictionary<int, ICPU>();
            idByCpu = new Dictionary<ICPU, int>();
            InitStructures();
            this.Log(LogLevel.Info, "System bus created.");
        }

        public void Register(IBusPeripheral peripheral, BusRangeRegistration registrationPoint)
        {
            var methods = PeripheralAccessMethods.CreateWithLock();
            FillAccessMethodsWithDefaultMethods(peripheral, ref methods);
            RegisterInner(peripheral, methods, registrationPoint);
        }

        public void Unregister(IBusPeripheral peripheral)
        {
            using(Machine.ObtainPausedState())
            {
                machine.UnregisterAsAChildOf(this, peripheral);
                UnregisterInner(peripheral);
            }
        }

        public void Register(IBusPeripheral peripheral, BusMultiRegistration registrationPoint)
        {
            var methods = PeripheralAccessMethods.CreateWithLock();
            FillAccessMethodsWithTaggedMethods(peripheral, registrationPoint.ConnectionRegionName, ref methods);
            RegisterInner(peripheral, methods, registrationPoint);
        }

        void IPeripheralRegister<IBusPeripheral, BusMultiRegistration>.Unregister(IBusPeripheral peripheral)
        {
            Unregister(peripheral);
        }

        public void Register(IKnownSize peripheral, BusPointRegistration registrationPoint)
        {
            Register(peripheral, new BusRangeRegistration(new Range(registrationPoint.StartingPoint, peripheral.Size), registrationPoint.Offset));
        }

        public void Unregister(IKnownSize peripheral)
        {
            Unregister((IBusPeripheral)peripheral);
        }

        public void Register(ICPU cpu, CPURegistrationPoint registrationPoint)
        {
            lock(cpuSync)
            {
                if(mappingsRemoved)
                {
                    throw new RegistrationException("Currently cannot register CPU after some memory mappings have been dynamically removed.");
                }
                if(!registrationPoint.Slot.HasValue)
                {
                    var i = 0;
                    while(cpuById.ContainsKey(i))
                    {
                        i++;
                    }
                    registrationPoint = new CPURegistrationPoint(i);
                }
                machine.RegisterAsAChildOf(this, cpu, registrationPoint);
                cpuById.Add(registrationPoint.Slot.Value, cpu);
                idByCpu.Add(cpu, registrationPoint.Slot.Value);
                foreach(var mapping in mappingsForPeripheral.SelectMany(x => x.Value))
                {
                    cpu.MapMemory(mapping);
                }
            }
        }

        public void Unregister(ICPU cpu)
        {
            using(machine.ObtainPausedState())
            {
                machine.UnregisterAsAChildOf(this, cpu);
                lock(cpuSync)
                {
                    var id = idByCpu[cpu];
                    idByCpu.Remove(cpu);
                    cpuById.Remove(id);
                }
            }
        }

        public void LogPeripheralAccess(IBusPeripheral busPeripheral, bool enable = true)
        {           
            peripherals.VisitAccessMethods(busPeripheral, pam =>
            {
                // first check whether logging is already enabled, method should be idempotent
                var loggingAlreadEnabled = pam.WriteByte.Target is HookWrapper;
                this.Log(LogLevel.Info, "Logging already enabled: {0}.", loggingAlreadEnabled);
                if(enable == loggingAlreadEnabled)
                {
                    return pam;
                }
                if(enable)
                {
                    pam.WriteByte = new BusAccess.ByteWriteMethod(new WriteLoggingWrapper<byte>(busPeripheral, new Action<long, byte>(pam.WriteByte)).Write);
                    pam.WriteWord = new BusAccess.WordWriteMethod(new WriteLoggingWrapper<ushort>(busPeripheral, new Action<long, ushort>(pam.WriteWord)).Write);
                    pam.WriteDoubleWord = new BusAccess.DoubleWordWriteMethod(new WriteLoggingWrapper<uint>(busPeripheral, new Action<long, uint>(pam.WriteDoubleWord)).Write);
                    pam.ReadByte = new BusAccess.ByteReadMethod(new ReadLoggingWrapper<byte>(busPeripheral, new Func<long, byte>(pam.ReadByte)).Read);
                    pam.ReadWord = new BusAccess.WordReadMethod(new ReadLoggingWrapper<ushort>(busPeripheral, new Func<long, ushort>(pam.ReadWord)).Read);
                    pam.ReadDoubleWord = new BusAccess.DoubleWordReadMethod(new ReadLoggingWrapper<uint>(busPeripheral, new Func<long, uint>(pam.ReadDoubleWord)).Read);
                    return pam;
                }
                else
                {
                    pam.WriteByte = new BusAccess.ByteWriteMethod(((WriteLoggingWrapper<byte>)pam.WriteByte.Target).OriginalMethod);
                    pam.WriteWord = new BusAccess.WordWriteMethod(((WriteLoggingWrapper<ushort>)pam.WriteWord.Target).OriginalMethod);
                    pam.WriteDoubleWord = new BusAccess.DoubleWordWriteMethod(((WriteLoggingWrapper<uint>)pam.WriteDoubleWord.Target).OriginalMethod);
                    pam.ReadByte = new BusAccess.ByteReadMethod(((ReadLoggingWrapper<byte>)pam.ReadByte.Target).OriginalMethod);
                    pam.ReadWord = new BusAccess.WordReadMethod(((ReadLoggingWrapper<ushort>)pam.ReadWord.Target).OriginalMethod);
                    pam.ReadDoubleWord = new BusAccess.DoubleWordReadMethod(((ReadLoggingWrapper<uint>)pam.ReadDoubleWord.Target).OriginalMethod);
                    return pam;
                }
            });
        }

        public IEnumerable<ICPU> GetCPUs()
        {
            lock(cpuSync)
            {
                return new ReadOnlyCollection<ICPU>(idByCpu.Keys.ToList());
            }
        }

        public int GetCPUId(ICPU cpu)
        {
            lock(cpuSync)
            {
                if(idByCpu.ContainsKey(cpu))
                {
                    return idByCpu[cpu];
                }
                throw new KeyNotFoundException("Given CPU is not registered.");
            }
        }

        public ICPU GetCurrentCPU()
        {
            ICPU cpu;
            if(!TryGetCurrentCPU(out cpu))
            {
                // TODO: inline
                throw new RecoverableException(CantFindCpuIdMessage);
            }
            return cpu;
        }

        public int GetCurrentCPUId()
        {
            int id;
            if(!TryGetCurrentCPUId(out id))
            {
                throw new RecoverableException(CantFindCpuIdMessage);
            }
            return id;
        }

        public bool TryGetCurrentCPUId(out int cpuId)
        {
            /* 
             * Because getting cpu id can possibly be a heavy operation, we cache the
             * obtained ID in the thread local storage. Note that we assume here that the
             * thread with such storage won't be used for another purposes than it was
             * used originally (i.e. cpu loop).
             */
            lock(cpuSync)
            {
                if(cachedCpuId.IsValueCreated)
                {
                    cpuId = cachedCpuId.Value;
                    return true;
                }
                foreach(var entry in cpuById)
                {
                    var candidate = entry.Value;
                    if(!candidate.OnPossessedThread)
                    {
                        continue;
                    }
                    cpuId = entry.Key;
                    cachedCpuId.Value = cpuId;
                    return true;
                }
                cpuId = -1;
                return false;
            }
        }

        public bool TryGetCurrentCPU(out ICPU cpu)
        {
            lock(cpuSync)
            {
                int id;
                if(TryGetCurrentCPUId(out id))
                {
                    cpu = cpuById[id];
                    return true;
                }
                cpu = null;
                return false;
            }
        }

        public void Unregister(long address)
        {
            // TODO: whole device or not?
            var peripheral = WhatIsAt(address);
            if(peripheral == null)
            {
                throw new RecoverableException(string.Format(
                    "There is no peripheral registered at 0x{0:X}.", address));
            }
            Unregister(peripheral.Peripheral);
        }

        public void Dispose()
        {
            cachedCpuId.Dispose();
            #if DEBUG
            peripherals.ShowStatistics();
            #endif
        }

        /// <summary>Checks what is at a given address.</summary>
        /// <param name="address">
        ///     A <see cref="Long"/> with the address to check.
        /// </param>
        /// <returns>
        ///     A peripheral which is at the given address.
        /// </returns>
        public IBusRegistered<IBusPeripheral> WhatIsAt(long address)
        {
            return peripherals.Peripherals.FirstOrDefault(x => x.RegistrationPoint.Range.Contains(address));
        }

        public IPeripheral WhatPeripheralIsAt(long address)
        {
            var registered = WhatIsAt(address);
            if(registered != null)
            {
                return registered.Peripheral;
            }
            return null;
        }

        public IBusRegistered<MappedMemory> FindMemory(long address)
        {
            return peripherals.Peripherals.Where(x => x.Peripheral is MappedMemory).Convert<IBusPeripheral, MappedMemory>().FirstOrDefault(x => x.RegistrationPoint.Range.Contains(address));
        }

        public IEnumerable<IBusRegistered<IMapped>> GetMappedPeripherals()
        {
            return peripherals.Peripherals.Where(x => x.Peripheral is IMapped).Convert<IBusPeripheral, IMapped>();
        }

        public void SilenceRange(Range range)
        {
            var silencer = new Silencer();
            Register(silencer, new BusRangeRegistration(range));
        }

        public void ReadBytes(long address, int count, byte[] destination, int startIndex, bool onlyMemory = false)
        {
            var targets = FindTargets(address, count);
            if(onlyMemory)
            {
                ThrowIfNotAllMemory(targets);
            }
            foreach(var target in targets)
            {
                var memory = target.What.Peripheral as MappedMemory;
                if(memory != null)
                {
                    checked
                    {
                        memory.ReadBytes(target.Offset - target.What.RegistrationPoint.Range.StartAddress + target.What.RegistrationPoint.Offset, (int)target.SourceLength, destination, (int)target.SourceIndex);
                    }
                }
                else
                {
                    for(var i = 0L; i < target.SourceLength; ++i)
                    {
                        destination[startIndex + target.SourceIndex + i] = ReadByte(target.Offset + i);
                    }
                }
            }
        }

        public byte[] ReadBytes(long address, int count, bool onlyMemory = false)
        {
            var result = new byte[count];
            ReadBytes(address, count, result, 0, onlyMemory);
            return result;
        }

        public void WriteBytes(byte[] bytes, long address, bool onlyMemory = false)
        {
            WriteBytes(bytes, address, bytes.Length, onlyMemory);
        }

        public void WriteBytes(byte[] bytes, long address, int startingIndex, long count, bool onlyMemory = false)
        {
            var targets = FindTargets(address, count);
            if(onlyMemory)
            {
                ThrowIfNotAllMemory(targets);
            }
            foreach(var target in targets)
            {
                var multibytePeripheral = target.What.Peripheral as IMultibyteWritePeripheral;
                if(multibytePeripheral != null)
                {
                    checked
                    {
                        multibytePeripheral.WriteBytes(target.Offset - target.What.RegistrationPoint.Range.StartAddress + target.What.RegistrationPoint.Offset, bytes, startingIndex + (int)target.SourceIndex, (int)target.SourceLength);
                    }
                }
                else
                {
                    for(var i = 0L; i < target.SourceLength; ++i)
                    {
                        WriteByte(target.Offset + i, bytes[target.SourceIndex + i]);
                    }
                }
            }
        }

        public void WriteBytes(byte[] bytes, long address, long count, bool onlyMemory = false)
        {
            WriteBytes(bytes, address, 0, count, onlyMemory);
        }

        public void ZeroRange(Range range)
        {
            var zeroBlock = new byte[1024 * 1024];
            var blocksNo = range.Size / zeroBlock.Length;
            for(var i = 0; i < blocksNo; i++)
            {
                WriteBytes(zeroBlock, range.StartAddress + i * zeroBlock.Length);
            }
            WriteBytes(zeroBlock, range.StartAddress + blocksNo * zeroBlock.Length, (int)range.Size % zeroBlock.Length);
        }

        public void ZeroRange(long from, long size)
        {
            ZeroRange(from.By(size));
        }

        public void LoadSymbolsFrom(string fileName)
        {
            Lookup.LoadELF(GetELFFromFile(fileName));
        }

        public void AddSymbol(Range address, string name, bool isThumb = false)
        {
            checked
            {
                Lookup.InsertSymbol(name, (uint)address.StartAddress, (uint)address.Size);
            }
        }

        public void LoadELF(string fileName, bool useVirtualAddress = false, bool allowLoadsOnlyToMemory = true, IControllableCPU cpu = null)
        {
            if(!Machine.IsPaused)
            {
                throw new RecoverableException("Cannot load ELF on an unpaused machine.");
            }
            if(cpu == null)
            {
                cpu = (IControllableCPU)GetCPUs().FirstOrDefault();
            }
            var elf = GetELFFromFile(fileName);
            var segmentsToLoad = elf.Segments.Where(x => x.Type == SegmentType.Load);
            foreach(var s in segmentsToLoad)
            {
                var contents = s.GetContents();
                var loadAddress = useVirtualAddress ? s.Address : s.PhysicalAddress;
                this.Log(LogLevel.Info,
                    "Loading segment of {0} bytes length at 0x{1:X}.",
                    s.Size,
                    loadAddress
                );
                this.WriteBytes(contents, loadAddress, allowLoadsOnlyToMemory);
                UpdateLowestLoadedAddress(loadAddress);
                this.DebugLog("Segment loaded.");
            }
            Lookup.LoadELF(elf);
            if(cpu != null)
            {
                cpu.InitFromElf(elf);
            }
            AddFingerprint(fileName);
        }

        public void LoadUImage(string fileName, IControllableCPU cpu = null)
        {
            if(!Machine.IsPaused)
            {
                throw new RecoverableException("Cannot load ELF on an unpaused machine.");
            }
            UImage uImage;
            if(cpu == null)
            {
                cpu = (IControllableCPU)GetCPUs().FirstOrDefault();
            }

            switch(UImageReader.TryLoad(fileName, out uImage))
            {
            case UImageResult.NotUImage:
                throw new RecoverableException(string.Format("Given file '{0}' is not a U-Boot image.", fileName));
            case UImageResult.BadChecksum:
                throw new RecoverableException(string.Format("Header checksum does not match for the U-Boot image '{0}'.", fileName));
            case UImageResult.NotSupportedImageType:
                throw new RecoverableException(string.Format("Given file '{0}' is not of a supported image type.", fileName));
            }
            byte[] toLoad;
            switch(uImage.TryGetImageData(out toLoad))
            {
            case ImageDataResult.BadChecksum:
                throw new RecoverableException("Bad image checksum, probably corrupted image.");
            case ImageDataResult.UnsupportedCompressionFormat:
                throw new RecoverableException(string.Format("Unsupported compression format '{0}'.", uImage.Compression));
            }
            WriteBytes(toLoad, uImage.LoadAddress);
            if(cpu != null)
            {
                cpu.InitFromUImage(uImage);
            }
            this.Log(LogLevel.Info, string.Format(
                "Loaded U-Boot image '{0}'\n" +
                "load address: 0x{1:X}\n" +
                "size:         {2}B = {3}B\n" +
                "timestamp:    {4}\n" +
                "entry point:  0x{5:X}\n" +
                "architecture: {6}\n" +
                "OS:           {7}", 
                uImage.Name,
                uImage.LoadAddress,
                uImage.Size, Misc.NormalizeBinary(uImage.Size),
                uImage.Timestamp,
                uImage.EntryPoint,
                uImage.Architecture,
                uImage.OperatingSystem
            ));
            AddFingerprint(fileName);
            UpdateLowestLoadedAddress(uImage.LoadAddress);
        }

        public void LoadBinary(string fileName, long loadPoint)
        {
            const int bufferSize = 100 * 1024;
            this.DebugLog(string.Format("Loading binary {0} at 0x{1:X}.", fileName, loadPoint));
            try
            {
                using(var reader = new FileStream(fileName, FileMode.Open))
                {
                    var buffer = new byte[bufferSize];
                    var written = 0;
                    var read = 0;
                    while((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        WriteBytes(buffer, loadPoint + written, read);
                        written += read;
                    }
                }
            }
            catch(IOException e)
            {
                throw new RecoverableException(string.Format("Exception while loading file {0}: {1}", fileName, e.Message));
            }
            AddFingerprint(fileName);
            UpdateLowestLoadedAddress(checked((uint)loadPoint));
            this.DebugLog("Binary loaded.");
        }

        public IEnumerable<BinaryFingerprint> GetLoadedFingerprints()
        {
            return binaryFingerprints.ToArray();
        }

        public BinaryFingerprint GetFingerprint(string fileName)
        {
            return new BinaryFingerprint(fileName);
        }

        public uint GetSymbolAddress(string symbolName)
        {
            IReadOnlyCollection<Symbol> symbols;
            if(!Lookup.TryGetSymbolsByName(symbolName, out symbols))
            {
                throw new RecoverableException(string.Format("No symbol with name `{0}` found.", symbolName));
            }
            if(symbols.Count > 1)
            {
                throw new RecoverableException(string.Format("Ambiguous symbol name: `{0}`.", symbolName));
            }
            return symbols.First().Start;

        }

        public string FindSymbolAt(uint offset)
        {
            Symbol symbol;
            if(Lookup.TryGetSymbolByAddress(offset, out symbol))
            {
                return symbol.Name;
            }
            return null;
        }

        public void MapMemory(IMappedSegment segment, IBusPeripheral owner, bool relative = true)
        {
            if(relative)
            {
                var wrappers = new List<MappedSegmentWrapper>();
                foreach(var registrationPoint in GetRegistrationPoints(owner))
                {
                    var wrapper = FromRegistrationPointToSegmentWrapper(segment, registrationPoint);
                    if(wrapper != null)
                    {
                        wrappers.Add(wrapper);
                    }
                }
                AddMappings(wrappers, owner);
            }
            else
            {
                AddMappings(new [] { new MappedSegmentWrapper(segment, 0, long.MaxValue) }, owner);
            }
        }

        public void UnmapMemory(long start, long size)
        {
            UnmapMemory(start.By(size));
        }

        public void UnmapMemory(Range range)
        {
            lock(cpuSync)
            {
                mappingsRemoved = true;
                foreach(var cpu in idByCpu.Keys)
                {
                    cpu.UnmapMemory(range);
                }
            }
        }

        public IEnumerable<BusRangeRegistration> GetRegistrationPoints(IBusPeripheral peripheral)
        {
            return peripherals.Peripherals.Where(x => x.Peripheral == peripheral).Select(x => x.RegistrationPoint);
        }

        public void ApplySVD(string path)
        {
            var svdDevice = new SVDDummyDevice(path, this);
            svdDevices.Add(svdDevice);
        }

        public void Tag(Range range, string tag, uint defaultValue = 0, bool pausing = false)
        {
            var intersectings = tags.Where(x => x.Key.Intersects(range)).ToArray();
            if(intersectings.Length == 0)
            {
                tags.Add(range, new TagEntry { Name = tag, DefaultValue = defaultValue });
                if(pausing)
                {
                    pausingTags.Add(tag);
                }
                return;
            }
            // tag splitting
            if(intersectings.Length != 1)
            {
                throw new RecoverableException(string.Format(
                    "Currently subtag has to be completely contained in other tag. Given one intersects with tags: {0}",
                    intersectings.Select(x => x.Value.Name).Aggregate((x, y) => x + ", " + y)));
            }
            var parentRange = intersectings[0].Key;
            var parentName = intersectings[0].Value.Name;
            var parentDefaultValue = intersectings[0].Value.DefaultValue;
            var parentPausing = pausingTags.Contains(parentName);
            if(!parentRange.Contains(range))
            {
                throw new RecoverableException(string.Format(
                    "Currently subtag has to be completely contained in other tag, in this case {0}.", parentName));
            }
            RemoveTag(parentRange.StartAddress);
            var parentRangeAfterSplitSizeLeft = range.StartAddress - parentRange.StartAddress;
            if(parentRangeAfterSplitSizeLeft > 0)
            {
                Tag(new Range(parentRange.StartAddress, parentRangeAfterSplitSizeLeft), parentName, parentDefaultValue, parentPausing);
            }
            var parentRangeAfterSplitSizeRight = parentRange.EndAddress - range.EndAddress;
            if(parentRangeAfterSplitSizeRight > 0)
            {
                Tag(new Range(range.EndAddress + 1, parentRangeAfterSplitSizeRight), parentName, parentDefaultValue, parentPausing);
            }
            Tag(range, string.Format("{0}/{1}", parentName, tag), defaultValue, pausing);
        }

        public void RemoveTag(long address)
        {
            var tagsToRemove = tags.Where(x => x.Key.Contains(address)).ToArray();
            if(tagsToRemove.Length == 0)
            {
                throw new RecoverableException(string.Format("There is no tag at address 0x{0:X}.", address));
            }
            foreach(var tag in tagsToRemove)
            {
                tags.Remove(tag.Key);
                pausingTags.Remove(tag.Value.Name);
            }
        }

        public void Clear()
        {
            ClearAll();
        }

        public void Reset()
        {
            LowestLoadedAddress = null;
            Lookup = new SymbolLookup();
        }

        public Machine Machine
        {
            get
            {
                return machine;
            }
        }

        public int UnexpectedReads
        {
            get
            {
                return Interlocked.CompareExchange(ref unexpectedReads, 0, 0);
            }
        }

        public int UnexpectedWrites
        {
            get
            {
                return Interlocked.CompareExchange(ref unexpectedWrites, 0, 0);
            }
        }

        public uint? LowestLoadedAddress { get; private set; }

        public IEnumerable<IRegistered<IBusPeripheral, BusRangeRegistration>> Children
        {
            get
            {
                foreach(var peripheral in peripherals.Peripherals)
                {
                    yield return peripheral;
                }
            }
        }

        public Endianess Endianess
        {
            get
            {
                return endianess;
            }
            set
            {
                if(peripheralRegistered)
                {
                    throw new RecoverableException("Currently one has to set endianess before any peripheral is registered.");
                }
                endianess = value;
            }
        }

        public SymbolLookup Lookup
        {
            get;
            private set;
        }

        public UnhandledAccessBehaviour UnhandledAccessBehaviour { get; set; }

        private void UnregisterInner(IBusPeripheral peripheral)
        {
            if(mappingsForPeripheral.ContainsKey(peripheral))
            {
                foreach(var mapping in mappingsForPeripheral[peripheral])
                {
                    UnmapMemory(new Range(mapping.StartingOffset, mapping.Size));
                }
                mappingsForPeripheral.Remove(peripheral);
            }
            peripherals.Remove(peripheral);
        }

        private void FillAccessMethodsWithTaggedMethods(IBusPeripheral peripheral, string tag, ref PeripheralAccessMethods methods)
        {
            methods.Peripheral = peripheral;

            var customAccessMethods = new Dictionary<Tuple<BusAccess.Method, BusAccess.Operation>, MethodInfo>();
            foreach(var method in peripheral.GetType().GetMethods())
            {
                Type signature = null;
                if(!Misc.TryGetMatchingSignature(BusAccess.Delegates, method, out signature))
                {
                    continue;
                }

                var accessGroupAttribute = (ConnectionRegionAttribute)method.GetCustomAttributes(typeof(ConnectionRegionAttribute), true).FirstOrDefault();
                if(accessGroupAttribute == null || accessGroupAttribute.Name != tag)
                {
                    continue;
                }

                var accessMethod = BusAccess.GetMethodFromSignature(signature);
                var accessOperation = BusAccess.GetOperationFromSignature(signature);

                var tuple = Tuple.Create(accessMethod, accessOperation);
                if(customAccessMethods.ContainsKey(tuple))
                {
                    throw new RegistrationException(string.Format("Only one method for operation {0} accessing {1} registers is allowed.", accessOperation, accessMethod));
                }

                customAccessMethods[tuple] = method;
                methods.SetMethod(method, peripheral, accessOperation, accessMethod);
            }

            FillAccessMethodsWithDefaultMethods(peripheral, ref methods);
        }

        private void FillAccessMethodsWithDefaultMethods(IBusPeripheral peripheral, ref PeripheralAccessMethods methods)
        {
            methods.Peripheral = peripheral;

            var bytePeripheral = peripheral as IBytePeripheral;
            var wordPeripheral = peripheral as IWordPeripheral;
            var dwordPeripheral = peripheral as IDoubleWordPeripheral;
            BytePeripheralWrapper byteWrapper = null;
            WordPeripheralWrapper wordWrapper = null;
            DoubleWordPeripheralWrapper dwordWrapper = null;

            if(methods.ReadByte != null)
            {
                byteWrapper = new BytePeripheralWrapper(methods.ReadByte, methods.WriteByte);
            }
            if(methods.ReadWord != null)
            {
                // why there are such wrappers? since device can be registered through
                // method specific registration points
                wordWrapper = new WordPeripheralWrapper(methods.ReadWord, methods.WriteWord);
            }
            if(methods.ReadDoubleWord != null)
            {
                dwordWrapper = new DoubleWordPeripheralWrapper(methods.ReadDoubleWord, methods.WriteDoubleWord);
            }

            if(bytePeripheral == null && wordPeripheral == null && dwordPeripheral == null && byteWrapper == null
               && wordWrapper == null && dwordWrapper == null)
            {
                throw new RegistrationException(string.Format("Cannot register peripheral {0}, it does not implement any of IBusPeripheral derived interfaces," +
                "nor any other methods were pointed.", peripheral));
            }

            Endianess periEndianess;
            var endianessAttributes = peripheral.GetType().GetCustomAttributes(typeof(EndianessAttribute), true);
            if(endianessAttributes.Length != 0)
            {
                periEndianess = ((EndianessAttribute)endianessAttributes[0]).Endianess;
            }
            else
            {
                periEndianess = Endianess;
            }

            var allowedTranslations = default(AllowedTranslation);
            var allowedTranslationsAttributes = peripheral.GetType().GetCustomAttributes(typeof(AllowedTranslationsAttribute), true);
            if(allowedTranslationsAttributes.Length != 0)
            {
                allowedTranslations = ((AllowedTranslationsAttribute)allowedTranslationsAttributes[0]).AllowedTranslations;
            }

            if(methods.ReadByte == null) // they are null or not always in pairs
            {
                if(bytePeripheral != null)
                {
                    methods.ReadByte = bytePeripheral.ReadByte;
                    methods.WriteByte = bytePeripheral.WriteByte;
                }
                else if(wordWrapper != null && (allowedTranslations & AllowedTranslation.ByteToWord) != 0)
                {
                    methods.ReadByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteReadMethod)wordWrapper.ReadByteUsingWord : wordWrapper.ReadByteUsingWordBigEndian;
                    methods.WriteByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteWriteMethod)wordWrapper.WriteByteUsingWord : wordWrapper.WriteByteUsingWordBigEndian;
                }
                else if(dwordWrapper != null && (allowedTranslations & AllowedTranslation.ByteToDoubleWord) != 0)
                {
                    methods.ReadByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteReadMethod)dwordWrapper.ReadByteUsingDword : dwordWrapper.ReadByteUsingDwordBigEndian;
                    methods.WriteByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteWriteMethod)dwordWrapper.WriteByteUsingDword : dwordWrapper.WriteByteUsingDwordBigEndian;
                }
                else if(wordPeripheral != null && (allowedTranslations & AllowedTranslation.ByteToWord) != 0)
                {
                    methods.ReadByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteReadMethod)wordPeripheral.ReadByteUsingWord : wordPeripheral.ReadByteUsingWordBigEndian;
                    methods.WriteByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteWriteMethod)wordPeripheral.WriteByteUsingWord : wordPeripheral.WriteByteUsingWordBigEndian;
                }
                else if(dwordPeripheral != null && (allowedTranslations & AllowedTranslation.ByteToDoubleWord) != 0)
                {
                    methods.ReadByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteReadMethod)dwordPeripheral.ReadByteUsingDword : dwordPeripheral.ReadByteUsingDwordBigEndian;
                    methods.WriteByte = periEndianess == Endianess.LittleEndian ? (BusAccess.ByteWriteMethod)dwordPeripheral.WriteByteUsingDword : dwordPeripheral.WriteByteUsingDwordBigEndian;
                }
                else
                {
                    methods.ReadByte = peripheral.ReadByteNotTranslated;
                    methods.WriteByte = peripheral.WriteByteNotTranslated;
                }
            }

            if(methods.ReadWord == null)
            {
                if(wordPeripheral != null)
                {
                    methods.ReadWord = wordPeripheral.ReadWord;
                    methods.WriteWord = wordPeripheral.WriteWord;
                }
                else if(dwordWrapper != null && (allowedTranslations & AllowedTranslation.WordToDoubleWord) != 0)
                {
                    methods.ReadWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordReadMethod)dwordWrapper.ReadWordUsingDword : dwordWrapper.ReadWordUsingDwordBigEndian;
                    methods.WriteWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordWriteMethod)dwordWrapper.WriteWordUsingDword : dwordWrapper.WriteWordUsingDwordBigEndian;
                }
                else if(byteWrapper != null && (allowedTranslations & AllowedTranslation.WordToByte) != 0)
                {
                    methods.ReadWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordReadMethod)byteWrapper.ReadWordUsingByte : byteWrapper.ReadWordUsingByteBigEndian;
                    methods.WriteWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordWriteMethod)byteWrapper.WriteWordUsingByte : byteWrapper.WriteWordUsingByteBigEndian;
                }
                else if(dwordPeripheral != null && (allowedTranslations & AllowedTranslation.WordToDoubleWord) != 0)
                {
                    methods.ReadWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordReadMethod)dwordPeripheral.ReadWordUsingDword : dwordPeripheral.ReadWordUsingDwordBigEndian;
                    methods.WriteWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordWriteMethod)dwordPeripheral.WriteWordUsingDword : dwordPeripheral.WriteWordUsingDwordBigEndian;
                }
                else if(bytePeripheral != null && (allowedTranslations & AllowedTranslation.WordToByte) != 0)
                {
                    methods.ReadWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordReadMethod)bytePeripheral.ReadWordUsingByte : bytePeripheral.ReadWordUsingByteBigEndian;
                    methods.WriteWord = periEndianess == Endianess.LittleEndian ? (BusAccess.WordWriteMethod)bytePeripheral.WriteWordUsingByte : bytePeripheral.WriteWordUsingByteBigEndian;
                }
                else
                {
                    methods.ReadWord = peripheral.ReadWordNotTranslated;
                    methods.WriteWord = peripheral.WriteWordNotTranslated;
                }
            }

            if(methods.ReadDoubleWord == null)
            {
                if(dwordPeripheral != null)
                {
                    methods.ReadDoubleWord = dwordPeripheral.ReadDoubleWord;
                    methods.WriteDoubleWord = dwordPeripheral.WriteDoubleWord;
                }
                else if(wordWrapper != null && (allowedTranslations & AllowedTranslation.DoubleWordToWord) != 0)
                {
                    methods.ReadDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordReadMethod)wordWrapper.ReadDoubleWordUsingWord : wordWrapper.ReadDoubleWordUsingWordBigEndian;
                    methods.WriteDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordWriteMethod)wordWrapper.WriteDoubleWordUsingWord : wordWrapper.WriteDoubleWordUsingWordBigEndian;
                }
                else if(byteWrapper != null && (allowedTranslations & AllowedTranslation.DoubleWordToByte) != 0)
                {
                    methods.ReadDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordReadMethod)byteWrapper.ReadDoubleWordUsingByte : byteWrapper.ReadDoubleWordUsingByteBigEndian;
                    methods.WriteDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordWriteMethod)byteWrapper.WriteDoubleWordUsingByte : byteWrapper.WriteDoubleWordUsingByteBigEndian;
                }
                else if(wordPeripheral != null && (allowedTranslations & AllowedTranslation.DoubleWordToWord) != 0)
                {
                    methods.ReadDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordReadMethod)wordPeripheral.ReadDoubleWordUsingWord : wordPeripheral.ReadDoubleWordUsingWordBigEndian;
                    methods.WriteDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordWriteMethod)wordPeripheral.WriteDoubleWordUsingWord : wordPeripheral.WriteDoubleWordUsingWordBigEndian;
                }
                else if(bytePeripheral != null && (allowedTranslations & AllowedTranslation.DoubleWordToByte) != 0)
                {
                    methods.ReadDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordReadMethod)bytePeripheral.ReadDoubleWordUsingByte : bytePeripheral.ReadDoubleWordUsingByteBigEndian;
                    methods.WriteDoubleWord = periEndianess == Endianess.LittleEndian ? (BusAccess.DoubleWordWriteMethod)bytePeripheral.WriteDoubleWordUsingByte : bytePeripheral.WriteDoubleWordUsingByteBigEndian;
                }
                else
                {
                    methods.ReadDoubleWord = peripheral.ReadDoubleWordNotTranslated;
                    methods.WriteDoubleWord = peripheral.WriteDoubleWordNotTranslated;
                }
            }

            peripheralRegistered = true;
        }

        private void RegisterInner(IBusPeripheral peripheral, PeripheralAccessMethods methods, BusRangeRegistration registrationPoint)
        {
            using(machine.ObtainPausedState())
            {
                var intersecting = peripherals.Peripherals.FirstOrDefault(x => x.RegistrationPoint.Range.Intersects(registrationPoint.Range));
                if(intersecting != null)
                {
                    throw new RegistrationException(string.Format(
                        "Given address {0} for peripheral {1} conflicts with address {2} of peripheral {3}", registrationPoint.Range, peripheral, intersecting.RegistrationPoint.Range, intersecting.Peripheral), "address");
                }
                // TODO: CheckMappings
                var registeredPeripheral = new BusRegistered<IBusPeripheral>(peripheral, registrationPoint);
                
                // we also have to put missing methods
                var absoluteAddressAware = peripheral as IAbsoluteAddressAware;
                if(absoluteAddressAware != null)
                {
                    methods.SetAbsoluteAddress = absoluteAddressAware.SetAbsoluteAddress;
                }
                peripherals.Add(registrationPoint.Range.StartAddress, registrationPoint.Range.EndAddress + 1, registeredPeripheral, methods);
                // let's add new mappings
                var mappedPeripheral = peripheral as IMapped;
                if(mappedPeripheral != null)
                {
                    var segments = mappedPeripheral.MappedSegments;
                    var mappings = segments.Select(x => FromRegistrationPointToSegmentWrapper(x, registrationPoint)).Where(x => x != null);
                    AddMappings(mappings, peripheral);
                }
                machine.RegisterAsAChildOf(this, peripheral, registrationPoint);
            }
        }

        private IEnumerable<PeripheralLookupResult> FindTargets(long address, long count)
        {
            var result = new List<PeripheralLookupResult>();
            var written = 0L;
            while(written < count)
            {
                var currentPosition = address + written;
                // what peripheral is at the current write position?
                var what = WhatIsAt(currentPosition);
                if(what == null)
                {
                    var holeStart = currentPosition;
                    // we can omit part of the array
                    // but how much?
                    var nextPeripheral = peripherals.Peripherals.OrderBy(x => x.RegistrationPoint.Range.StartAddress).FirstOrDefault(x => x.RegistrationPoint.Range.StartAddress > currentPosition);
                    if(nextPeripheral == null)
                    {
                        // hole reaches the end of the required range
                        written = count;
                    }
                    else
                    {
                        written += Math.Min(nextPeripheral.RegistrationPoint.Range.StartAddress - currentPosition, count - written);
                    }
                    var holeSize = address + written - currentPosition;
                    this.Log(LogLevel.Warning, "Tried to access bytes at non-existing peripheral in range {0}.", new Range(holeStart, holeSize));
                    continue;
                }
                var toWrite = Math.Min(count - written, what.RegistrationPoint.Range.EndAddress - currentPosition + 1);
                var singleResult = new PeripheralLookupResult();
                singleResult.What = what;
                singleResult.SourceIndex = written;
                singleResult.SourceLength = toWrite;
                singleResult.Offset = currentPosition;
                written += toWrite;
                result.Add(singleResult);
            }

            return result;            
        }

        private static void ThrowIfNotAllMemory(IEnumerable<PeripheralLookupResult> targets)
        {
            foreach(var target in targets)
            {
                var iMemory = target.What.Peripheral as IMemory;
                var redirector = target.What.Peripheral as Redirector;
                if(iMemory == null && redirector == null)
                {
                    throw new RecoverableException(String.Format("Tried to access {0} but only memory accesses were allowed.", target.What.Peripheral));
                }
            }
        }

        private static ELF<uint> GetELFFromFile(string fileName)
        {
            ELF<uint> elf;
            try
            {
                elf = ELFReader.Load<uint>(fileName);
            }
            catch(Exception e)
            {
                // ELF creating exception are recoverable in the sense of emulator state
                throw new RecoverableException(string.Format("Error while loading ELF: {0}.", e.Message), e);
            }
            return elf;
        }

        private void ClearAll()
        {
            lock(cpuSync)
            {
                foreach(var group in Machine.PeripheralsGroups.ActiveGroups)
                {
                    group.Unregister();
                }

                foreach(var p in peripherals.Peripherals.Select(x => x.Peripheral).Distinct().Union(GetCPUs().Cast<IPeripheral>()).ToList())
                {
                    Machine.UnregisterFromParent(p);
                }

                mappingsRemoved = false;
                InitStructures();
            }
        }

        private void UpdateLowestLoadedAddress(uint withValue)
        {
            if(!LowestLoadedAddress.HasValue)
            {
                LowestLoadedAddress = withValue;
                return;
            }
            LowestLoadedAddress = Math.Min(LowestLoadedAddress.Value, withValue);
        }

        private void AddFingerprint(string fileName)
        {
            binaryFingerprints.Add(new BinaryFingerprint(fileName));
        }

        private void InitStructures()
        {
            cpuById.Clear();
            idByCpu.Clear();
            Lookup = new SymbolLookup();
            cachedCpuId = new ThreadLocal<int>();
            peripherals = new PeripheralCollection(this);
            mappingsForPeripheral = new Dictionary<IBusPeripheral, List<MappedSegmentWrapper>>();
            tags = new Dictionary<Range, TagEntry>();
            svdDevices = new List<SVDDummyDevice>();
            pausingTags = new HashSet<string>();
        }

        private List<MappedMemory> ObtainMemoryList()
        {
            return peripherals.Peripherals.Where(x => x.Peripheral is MappedMemory).OrderBy(x => x.RegistrationPoint.Range.StartAddress).
                Select(x => x.Peripheral).Cast<MappedMemory>().Distinct().ToList();
        }

        private bool TryGetPCForCPU(ICPU cpu, out long pc)
        {
            var controllableCpu = cpu as IControllableCPU;
            if(controllableCpu == null)
            {
                pc = 0;
                return false;
            }
            pc = controllableCpu.PC;
            return true;

        }

        private void AddMappings(IEnumerable<MappedSegmentWrapper> newMappings, IBusPeripheral owner)
        {
            lock(cpuSync)
            {
                var mappingsList = newMappings.ToList();
                {
                    if(mappingsForPeripheral.ContainsKey(owner))
                    {
                        mappingsForPeripheral[owner].AddRange(newMappings);
                    }
                    else
                    {
                        mappingsForPeripheral[owner] = mappingsList;
                    }
                    // old mappings are given to the CPU in the moment of its registration
                    foreach(var cpu in idByCpu.Keys)
                    {
                        foreach(var mapping in mappingsList)
                        {
                            cpu.MapMemory(mapping);
                        }
                    }
                }
            }
        }

        private string TryGetTag(long address, out uint defaultValue)
        {
            var tag = tags.FirstOrDefault(x => x.Key.Contains(address));
            defaultValue = default(uint);
            if(tag.Key == Range.Empty)
            {
                return null;
            }
            defaultValue = tag.Value.DefaultValue;
            return tag.Value.Name;
        }

        private string EnterTag(string str, long address, out bool tagEntered, out uint defaultValue)
        {
            // TODO: also pausing here in a bit hacky way
            var tag = TryGetTag(address, out defaultValue);
            if(tag == null)
            {
                tagEntered = false;
                return str;
            }
            tagEntered = true;
            if(pausingTags.Contains(tag))
            {
                machine.Pause();
            }
            return string.Format("(tag: '{0}') {1}", tag, str);
        }

        private string DecorateWithCPUNumberAndPC(string str)
        {
            var cpuAppended = false;
            var builder = new StringBuilder(str.Length + 27);
            builder.Append('[');
            ICPU cpu;
            if(!TryGetCurrentCPU(out cpu))
            {
                return str;
            }
            builder.Append("CPU");
            builder.Append(GetCPUId(cpu));
            cpuAppended = true;
            long pc;
            if(TryGetPCForCPU(cpu, out pc))
            {
                if(cpuAppended)
                {
                    builder.Append(": ");
                }
                builder.AppendFormat("0x{0:X}", pc);
            }
            builder.Append("] ");
            builder.Append(str);
            return builder.ToString();
        }

        private uint ReportNonExistingRead(long address, string type)
        {
            Interlocked.Increment(ref unexpectedReads);
            bool tagged;
            uint defaultValue;
            var warning = EnterTag(NonExistingRead, address, out tagged, out defaultValue);
            warning = DecorateWithCPUNumberAndPC(warning);
            if(UnhandledAccessBehaviour == UnhandledAccessBehaviour.DoNotReport)
            {
                return defaultValue;
            }
            if((UnhandledAccessBehaviour == UnhandledAccessBehaviour.ReportIfTagged && !tagged)
                || (UnhandledAccessBehaviour == UnhandledAccessBehaviour.ReportIfNotTagged && tagged))
            {
                return defaultValue;
            }
            if(tagged)
            {
                this.Log(LogLevel.Warning, warning.TrimEnd('.') + ", tagged - returning 0x{2:X}.", address, type, defaultValue);
            }
            else
            {
                uint value;
                foreach(var svdDevice in svdDevices)
                {
                    if(svdDevice.ReadAccess(address, out value))
                    {
                        return value;
                    }
                }
                this.Log(LogLevel.Warning, warning, address, type);
            }
            return defaultValue;
        }

        private void ReportNonExistingWrite(long address, uint value, string type)
        {
            Interlocked.Increment(ref unexpectedWrites);
            if(UnhandledAccessBehaviour == UnhandledAccessBehaviour.DoNotReport)
            {
                return;
            }
            bool tagged;
            uint dummy;
            var warning = EnterTag(NonExistingWrite, address, out tagged, out dummy);
            warning = DecorateWithCPUNumberAndPC(warning);
            if((UnhandledAccessBehaviour == UnhandledAccessBehaviour.ReportIfTagged && !tagged)
                || (UnhandledAccessBehaviour == UnhandledAccessBehaviour.ReportIfNotTagged && tagged))
            {
                return;
            }
            foreach(var svdDevice in svdDevices)
            {
                if(svdDevice.WriteAccess(address, value, type))
                {
                    return;
                }
            }
            this.Log(LogLevel.Warning, warning, address, value, type);
        }

        private static MappedSegmentWrapper FromRegistrationPointToSegmentWrapper(IMappedSegment segment, BusRangeRegistration registrationPoint)
        {
            var desiredSize = Math.Min(segment.Size, registrationPoint.Range.Size + registrationPoint.Offset - segment.StartingOffset);
            if(desiredSize <= 0)
            {
                return null;
            }
            return new MappedSegmentWrapper(segment, registrationPoint.Range.StartAddress - registrationPoint.Offset, desiredSize);
        }

        private PeripheralCollection peripherals;
        private Dictionary<IBusPeripheral, List<MappedSegmentWrapper>> mappingsForPeripheral;
        private bool mappingsRemoved;
        private bool peripheralRegistered;
        private Endianess endianess;
        private readonly Dictionary<ICPU, int> idByCpu;
        private readonly Dictionary<int, ICPU> cpuById;

        [Constructor]
        private ThreadLocal<int> cachedCpuId;
        private object cpuSync;
        private Dictionary<Range, TagEntry> tags;
        private List<SVDDummyDevice> svdDevices;
        private HashSet<string> pausingTags;
        private readonly List<BinaryFingerprint> binaryFingerprints;
        private readonly Machine machine;
        private const string NonExistingRead = "Read{1} from non existing peripheral at 0x{0:X}.";
        private const string NonExistingWrite = "Write{2} to non existing peripheral at 0x{0:X}, value 0x{1:X}";
        private const string IOExceptionMessage = "I/O error while loading ELF: {0}.";
        private const string CantFindCpuIdMessage = "Can't verify current CPU in the given context";
        private const bool Overlap = true;
        // TODO

        private int unexpectedReads;
        private int unexpectedWrites;

        private struct PeripheralLookupResult
        {
            public IBusRegistered<IBusPeripheral> What;
            public long Offset;
            public long SourceIndex;
            public long SourceLength;
        }

        private struct TagEntry
        {
            public string Name;
            public uint DefaultValue;
        }

        private class MappedSegmentWrapper : IMappedSegment
        {
            public MappedSegmentWrapper(IMappedSegment wrappedSegment, long peripheralOffset, long maximumSize)
            {
                this.wrappedSegment = wrappedSegment;
                this.peripheralOffset = peripheralOffset;
                usedSize = Math.Min(maximumSize, wrappedSegment.Size);
            }

            public void Touch()
            {
                wrappedSegment.Touch();
            }

            public override string ToString()
            {
                return string.Format("[MappedSegmentWrapper: StartingOffset=0x{0:X}, Size=0x{1:X}, OriginalStartingOffset=0x{2:X}, PeripheralOffset=0x{3:X}]",
                    StartingOffset, Size, OriginalStartingOffset, PeripheralOffset);
            }

            public long StartingOffset
            {
                get
                {
                    return peripheralOffset + wrappedSegment.StartingOffset;
                }
            }

            public long Size
            {
                get
                {
                    return usedSize;
                }
            }

            public IntPtr Pointer
            {
                get
                {
                    return wrappedSegment.Pointer;
                }
            }

            public long OriginalStartingOffset
            {
                get
                {
                    return wrappedSegment.StartingOffset;
                }
            }

            public long PeripheralOffset
            {
                get
                {
                    return peripheralOffset;
                }
            }


            private readonly IMappedSegment wrappedSegment;
            private readonly long peripheralOffset;
            private readonly long usedSize;
        }
    }

}

