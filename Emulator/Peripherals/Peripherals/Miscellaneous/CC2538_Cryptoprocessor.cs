//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Emul8.Core;
using Emul8.Core.Structure.Registers;
using Emul8.Logging;
using Emul8.Peripherals.Bus;

namespace Emul8.Peripherals.Miscellaneous
{
    public sealed class CC2538_Cryptoprocessor : IDoubleWordPeripheral, IKnownSize
    {
        public CC2538_Cryptoprocessor(Machine machine)
        {
            this.machine = machine;
            keys = new byte[NumberOfKeys][];
            keyStoreWriteArea = new bool[NumberOfKeys];
            Interrupt = new GPIO();

            var keyStoreWrittenRegister = new DoubleWordRegister(this);
            var keyStoreWriteAreaRegister = new DoubleWordRegister(this);
            for(var i = 0; i < keys.Length; i++)
            {
                var j = i;
                keyStoreWrittenRegister.DefineFlagField(i, writeCallback: (_, value) => { if(value) keys[j] = null; }, valueProviderCallback: _ => keys[j] != null, name: "RAM_AREA_WRITTEN" + i);
                keyStoreWriteAreaRegister.DefineFlagField(i, writeCallback: (_, value) => keyStoreWriteArea[j] = value, valueProviderCallback: _ => keyStoreWriteArea[j], name: "RAM_AREA" + i);
            }

            var registersMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.DmaChannel0Control, new DoubleWordRegister(this)
                    .WithFlag(0, out dmaInputChannelEnabled, name: "EN")
                    .WithFlag(1, name: "PRIO") // priority is not handled
                },
                {(long)Registers.DmaChannel0ExternalAddress, new DoubleWordRegister(this)
                    .WithValueField(0, 32, out dmaInputAddress)
                },
                {(long)Registers.DmaChannel0Length, new DoubleWordRegister(this)
                    .WithValueField(0, 15, writeCallback: (_, value) => DoInputTransfer((int)value))
                },
                {(long)Registers.KeyStoreWriteArea, keyStoreWriteAreaRegister},
                {(long)Registers.KeyStoreWrittenArea, keyStoreWrittenRegister},
                {(long)Registers.KeyStoreSize, new DoubleWordRegister(this)
                    .WithEnumField(0, 3, out keySize)
                },
                {(long)Registers.AlgorithmSelection, new DoubleWordRegister(this)
                    .WithEnumField(0, 3, out dmaDestination, name: "KEY-STORE AES HASH")
                    .WithFlag(31, name: "TAG")
                },
                {(long)Registers.InterruptConfiguration, new DoubleWordRegister(this)
                    .WithFlag(0, out interruptIsLevel)
                },
                {(long)Registers.InterruptEnable, new DoubleWordRegister(this)
                    .WithFlag(0, out resultInterruptEnabled)
                    .WithFlag(1, out dmaDoneInterruptEnabled)
                },
                {(long)Registers.InterruptClear, new DoubleWordRegister(this)
                    .WithFlag(0, writeCallback: (_, value) => { if(value) { resultInterrupt = false; RefreshInterrupts(); } }, valueProviderCallback: _ => false )
                    .WithFlag(1, writeCallback: (_, value) => { if(value) { dmaDoneInterrupt = false; RefreshInterrupts(); } }, valueProviderCallback: _ => false )
                    .WithFlag(29, FieldMode.Read, name: "KEY_ST_RD_ERR")
                    .WithFlag(30, FieldMode.Read, name: "KEY_ST_WR_ERR")
                    .WithFlag(31, FieldMode.Read, name: "DMA_BUS_ERR")
                },
                {(long)Registers.InterrptStatus, new DoubleWordRegister(this)
                    .WithFlag(0, FieldMode.Read, valueProviderCallback: _ => resultInterrupt, name: "RESULT_AV")
                    .WithFlag(1, FieldMode.Read, valueProviderCallback: _ => dmaDoneInterrupt, name: "DMA_IN_DONE")
                }
            };

            registers = new DoubleWordRegisterCollection(this, registersMap);
        }

        public uint ReadDoubleWord(long offset)
        {
            uint result;
            if(!registers.TryRead(offset, out result))
            {
                this.Log(LogLevel.Warning, "Unhandled read at 0x{0:X}.", offset);
                machine.Pause();
            }
            return result;
        }

        public void Reset()
        {
            registers.Reset();
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            if(!registers.TryWrite(offset, value))
            {
                this.Log(LogLevel.Warning, "Unhandled write to 0x{0:X}, value 0x{1:X}.", offset, value);
                machine.Pause();
            }
        }

        public long Size 
        {
            get
            {
                return 0x800;
            }
        }

        public GPIO Interrupt { get; private set; }

        private void RefreshInterrupts()
        {
            var value = (resultInterruptEnabled.Value && resultInterrupt) || (dmaDoneInterruptEnabled.Value && dmaDoneInterrupt);
            this.Log(LogLevel.Info, "Setting to {0}.", value);
            Interrupt.Set(value);
            if(!interruptIsLevel.Value)
            {
                dmaDoneInterrupt = false;
                resultInterrupt = false;
                Interrupt.Unset();
            }
        }

        private void DoInputTransfer(int length)
        {
            var placeInKeyStore = keyStoreWriteArea.Sum(x => x ? 1 : 0) * KeyEntrySizeInBytes;
            if(length != placeInKeyStore)
            {
                this.Log(LogLevel.Warning, "Not enough place in key store ({0}B) for the given transfer length ({1}B), ignoring transfer length.", placeInKeyStore, length);
            }

            for(var i = 0; i < keys.Length; i++)
            {
                if(!keyStoreWriteArea[i])
                {
                    continue;
                }
                if(keys[i] != null)
                {
                    this.Log(LogLevel.Warning, "Trying to write a key to a non empty key store, ignoring.");
                    continue; // TODO: this probably should result in some error
                }
                if(length <= 0)
                {
                    break;
                }
                var data = machine.SystemBus.ReadBytes(dmaInputAddress.Value, KeyEntrySizeInBytes);
                keys[i] = data;
                length -= KeyEntrySizeInBytes;
                dmaInputAddress.Value += KeyEntrySizeInBytes;
            }
            resultInterrupt = true;
            RefreshInterrupts();
        }

        private bool dmaDoneInterrupt;
        private bool resultInterrupt;
        private IValueRegisterField dmaInputAddress;
        private readonly bool[] keyStoreWriteArea;
        private readonly IFlagRegisterField dmaInputChannelEnabled;
        private readonly IEnumRegisterField<KeySize> keySize;
        private readonly IEnumRegisterField<DmaDestination> dmaDestination;
        private readonly IFlagRegisterField interruptIsLevel;
        private readonly IFlagRegisterField resultInterruptEnabled;
        private readonly IFlagRegisterField dmaDoneInterruptEnabled;
        private readonly DoubleWordRegisterCollection registers;
        private readonly byte[][] keys;
        private readonly Machine machine;

        private const int NumberOfKeys = 8;
        private const int KeyEntrySizeInBytes = 16;

        private enum Registers : uint
        {
            DmaChannel0Control = 0x0, // DMAC_CH0_CTRL
            DmaChannel0ExternalAddress = 0x4, // DMAC_CH0_EXTADDR
            DmaChannel0Length = 0xC, // DMAC_CH0_DMALENGTH
            KeyStoreWriteArea = 0x400, // AES_KEY_STORE_WRITE_AREA
            KeyStoreWrittenArea = 0x404, // AES_KEY_STORE_WRITTEN_AREA
            KeyStoreSize = 0x408, // AES_KEY_STORE_SIZE
            AlgorithmSelection = 0x700, // CTRL_ALG_SEL
            InterruptConfiguration = 0x780, // CTRL_INT_CFG
            InterruptEnable = 0x784, // CTRL_INT_EN
            InterruptClear = 0x788, // CTRL_INT_CLR
            InterrptStatus = 0x790, // CTRL_INT_STAT
        }

        private enum DmaDestination
        {
            KeyStore = 1,
            Aes = 2,
            HashEngine = 4
        }

        private enum KeySize
        {
            Bits128,
            Bits192,
            Bits256
        }
    }
}
