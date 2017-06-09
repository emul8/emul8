//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Emul8.Core;
using Emul8.Core.Structure.Registers;
using Emul8.Logging;
using Emul8.Peripherals.Bus;
using Emul8.Utilities;

namespace Emul8.Peripherals.Miscellaneous
{
    public sealed class CC2538_Cryptoprocessor : IDoubleWordPeripheral, IKnownSize
    {
        public CC2538_Cryptoprocessor(Machine machine)
        {
            this.machine = machine;
            Interrupt = new GPIO();

            var keyStoreWrittenRegister = new DoubleWordRegister(this);
            var keyStoreWriteAreaRegister = new DoubleWordRegister(this);
            for(var i = 0; i < NumberOfKeys; i++)
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
                    .WithValueField(0, 15, writeCallback: (_, value) => DoInputTransfer((int)value), valueProviderCallback: _ => 0)
                },
                {(long)Registers.DmaChannel1Control, new DoubleWordRegister(this)
                    .WithFlag(0, out dmaOutputChannelEnabled, name: "EN")
                    .WithFlag(1, name: "PRIO") // priority is not handled
                },
                {(long)Registers.DmaChannel1ExternalAddress, new DoubleWordRegister(this)
                    .WithValueField(0, 32, out dmaOutputAddress)
                },
                {(long)Registers.DmaChannel1Length, new DoubleWordRegister(this)
                    .WithValueField(0, 15, writeCallback: (_, value) => DoOutputTransfer((int)value), valueProviderCallback: _ => 0)
                },
                {(long)Registers.KeyStoreWriteArea, keyStoreWriteAreaRegister},
                {(long)Registers.KeyStoreWrittenArea, keyStoreWrittenRegister},
                {(long)Registers.KeyStoreSize, new DoubleWordRegister(this)
                    .WithEnumField(0, 3, out keySize)
                },
                {(long)Registers.KeyStoreReadArea, new DoubleWordRegister(this)
                    .WithValueField(0, 4, out selectedKey)
                    .WithFlag(31, FieldMode.Read, name: "BUSY", valueProviderCallback: _ => false)
                },
                {(long)Registers.AesControl, new DoubleWordRegister(this)
                    .WithEnumField(2, 1, out direction)
                    .WithFlag(5, out cbcEnabled)
                },
                {(long)Registers.AesCryptoLength0, new DoubleWordRegister(this)
                    .WithValueField(0, 32, FieldMode.Write, writeCallback: (_, value) => aesOperationLength = checked((int)value))
                },
                {(long)Registers.AesCryptoLength1, new DoubleWordRegister(this)
                    .WithValueField(0, 29, FieldMode.Write, writeCallback: (_, value) => { if(value != 0) this.Log(LogLevel.Error, "Unsupported crypto length that spans more than one register."); })
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
                    .WithFlag(30, writeCallback: (_, value) => { if(value) { keyStoreWriteErrorInterrupt = false; RefreshInterrupts(); } }, valueProviderCallback: _ => false, name: "KEY_ST_WR_ERR")
                    .WithFlag(31, FieldMode.Read, name: "DMA_BUS_ERR")
                },
                {(long)Registers.InterruptStatus, new DoubleWordRegister(this)
                    .WithFlag(0, FieldMode.Read, valueProviderCallback: _ => resultInterrupt, name: "RESULT_AV")
                    .WithFlag(1, FieldMode.Read, valueProviderCallback: _ => dmaDoneInterrupt, name: "DMA_IN_DONE")
                    .WithFlag(30, FieldMode.Read, valueProviderCallback: _ => keyStoreWriteErrorInterrupt, name: "KEY_ST_WR_ERR")
                },
                {(long)Registers.AesAuthLength, new DoubleWordRegister(this)
                    .WithValueField(0, 32, out aesAuthLength)
                }
            };

            for(var i = 0; i < 4; i++)
            {
                var j = i;
                var ivRegister = new DoubleWordRegister(this);
                ivRegister.DefineValueField(0, 32, writeCallback: (_, value) => BitConverter.GetBytes(value).CopyTo(inputVector, j * 4),
                                            valueProviderCallback: _ => BitConverter.ToUInt32(inputVector, j * 4));
                registersMap.Add((long)Registers.AesInputVector + 4 * i, ivRegister);
            }

            registers = new DoubleWordRegisterCollection(this, registersMap);
            Reset();
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void Reset()
        {
            registers.Reset();

            aesOperationLength = 0;

            keyStoreWriteErrorInterrupt = false;
            dmaDoneInterrupt = false;
            resultInterrupt = false;
            RefreshInterrupts();

            keys = new byte[NumberOfKeys][];
            keyStoreWriteArea = new bool[NumberOfKeys];
            inputVector = new byte[AesBlockSizeInBytes];
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
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
            var value = (resultInterruptEnabled.Value && resultInterrupt) || (dmaDoneInterruptEnabled.Value && dmaDoneInterrupt) || keyStoreWriteErrorInterrupt;
            this.Log(LogLevel.Debug, "Setting Interrupt to {0}.", value);
            Interrupt.Set(value);
            if(!interruptIsLevel.Value)
            {
                keyStoreWriteErrorInterrupt = false;
                dmaDoneInterrupt = false;
                resultInterrupt = false;
                Interrupt.Unset();
            }
        }

        private void DoInputTransfer(int length)
        {
            if(!dmaInputChannelEnabled.Value)
            {
                return;
            }
            switch(dmaDestination.Value)
            {
            case DmaDestination.KeyStore:
                HandleKeyTransfer(length);
                break;
            case DmaDestination.Aes:
                return; // the real crypto operation will start on output transfer
            case DmaDestination.HashEngine:
                this.Log(LogLevel.Error, "Hash engine is not supported.");
                return;
            default:
                throw new InvalidOperationException("Should not reach here.");
            }

            resultInterrupt = true;
            dmaDoneInterrupt = true;
            RefreshInterrupts();
        }

        private void HandleKeyTransfer(int length)
        {
            var totalNumberOfActivatedSlots = keyStoreWriteArea.Sum(x => x ? 1 : 0);
            var keyWriteSlotIndex = keyStoreWriteArea.IndexOf(x => true);
            var numberOfConsecutiveSlots = keyStoreWriteArea.Skip(keyWriteSlotIndex).TakeWhile(x => x == true).Count();

            if(totalNumberOfActivatedSlots != numberOfConsecutiveSlots)
            {
                this.Log(LogLevel.Warning, "Bits in key store write area are not set consecutively: {0}, ignoring transfer.", BitHelper.GetSetBitsPretty(BitHelper.GetValueFromBitsArray(keyStoreWriteArea)));
                keyStoreWriteErrorInterrupt = true;
                RefreshInterrupts();
                return;
            }

            if(length != numberOfConsecutiveSlots * KeyEntrySizeInBytes)
            {
                this.Log(LogLevel.Warning, "Transfer length {0}B is not consistent with the number selected slots: {1} (each of size {2}B). Ignoring transfer", length, numberOfConsecutiveSlots, KeyEntrySizeInBytes);
                keyStoreWriteErrorInterrupt = true;
                RefreshInterrupts();
                return;
            }

            var nonEmptyKeyStoreSlots = keys.Skip(keyWriteSlotIndex).Take(numberOfConsecutiveSlots).Select((v, i) => new { v, i }).Where(x => x.v != null).Select(x => x.i.ToString()).ToList();
            if(nonEmptyKeyStoreSlots.Count > 0)
            {
                this.Log(LogLevel.Warning, "Trying to write a key to a non empty key store: {0}, ignoring transfer.", string.Join(", ", nonEmptyKeyStoreSlots));
                keyStoreWriteErrorInterrupt = true;
                RefreshInterrupts();
                return;
            }

            for(var i = keyWriteSlotIndex; i < numberOfConsecutiveSlots; i++)
            {
                keys[i] = machine.SystemBus.ReadBytes(dmaInputAddress.Value, KeyEntrySizeInBytes);
                dmaInputAddress.Value += KeyEntrySizeInBytes;
            }
        }

        private void DoOutputTransfer(int length)
        {
            if(!dmaOutputChannelEnabled.Value)
            {
                return;
            }

            if(dmaDestination.Value != DmaDestination.Aes)
            {
                this.Log(LogLevel.Error, "Not implemented output transfer destination.");
                return;
            }

            // here the real cipher operation begins
            if(length != aesOperationLength)
            {
                this.Log(LogLevel.Error, "AES operation in which dma length is different than aes length is not supported.");
                return;
            }
            if(!cbcEnabled.Value)
            {
                this.Log(LogLevel.Error, "Unimplemented cipher mode.");
                return;
            }

            using(var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                var key = GetSelectedKey();

                var encryptorDecryptor = direction.Value == Direction.Encryption ? aes.CreateEncryptor(key, inputVector) : aes.CreateDecryptor(key, inputVector);
                using(var memoryStream = new MemoryStream())
                {
                    using(var cryptoStream = new CryptoStream(memoryStream, encryptorDecryptor, CryptoStreamMode.Write))
                    {
                        var input = machine.SystemBus.ReadBytes(dmaInputAddress.Value, length);
                        dmaInputAddress.Value += (uint)length;
                        cryptoStream.Write(input, 0, input.Length);
                    }
                    var output = memoryStream.ToArray();
                    machine.SystemBus.WriteBytes(output, dmaOutputAddress.Value);
                    dmaOutputAddress.Value += (uint)length;
                }
            }

            dmaDoneInterrupt = true;
            resultInterrupt = true;
            RefreshInterrupts();
        }

        private byte[] GetSelectedKey()
        {
            byte[] result;
            
            switch(keySize.Value)
            {
            case KeySize.Bits128:
                return keys[selectedKey.Value];
            case KeySize.Bits192:
                result = new byte[24];
                Array.Copy(keys[selectedKey.Value + 1], 0, result, 16, 8);
                break;
            case KeySize.Bits256:
                result = new byte[32];
                keys[selectedKey.Value + 1].CopyTo(result, 16);
                break;
            default:
                this.Log(LogLevel.Warning, "Reserved key size value used instead of the proper value.");
                return new byte[16];
            }
            Array.Copy(keys[selectedKey.Value], result, 16);
            return result;
        }

        private bool dmaDoneInterrupt;
        private bool resultInterrupt;
        private bool keyStoreWriteErrorInterrupt;
        private int aesOperationLength;
        private byte[] inputVector;
        private bool[] keyStoreWriteArea;
        private byte[][] keys;
        private readonly IFlagRegisterField cbcEnabled;
        private readonly IEnumRegisterField<Direction> direction;
        private readonly IValueRegisterField aesAuthLength;
        private readonly IValueRegisterField dmaInputAddress;
        private readonly IValueRegisterField dmaOutputAddress;
        private readonly IValueRegisterField selectedKey;
        private readonly IFlagRegisterField dmaInputChannelEnabled;
        private readonly IFlagRegisterField dmaOutputChannelEnabled;
        private readonly IEnumRegisterField<KeySize> keySize;
        private readonly IEnumRegisterField<DmaDestination> dmaDestination;
        private readonly IFlagRegisterField interruptIsLevel;
        private readonly IFlagRegisterField resultInterruptEnabled;
        private readonly IFlagRegisterField dmaDoneInterruptEnabled;
        private readonly DoubleWordRegisterCollection registers;
        private readonly Machine machine;

        private const int NumberOfKeys = 8;
        private const int KeyEntrySizeInBytes = 16;
        private const int AesBlockSizeInBytes = 16;

        private enum Registers : uint
        {
            DmaChannel0Control = 0x0, // DMAC_CH0_CTRL
            DmaChannel0ExternalAddress = 0x4, // DMAC_CH0_EXTADDR
            DmaChannel0Length = 0xC, // DMAC_CH0_DMALENGTH
            DmaChannel1Control = 0x20, // DMAC_CH1_CTRL
            DmaChannel1ExternalAddress = 0x24, // DMAC_CH1_EXTADDR
            DmaChannel1Length = 0x2C, // DMAC_CH1_DMALENGTH
            KeyStoreWriteArea = 0x400, // AES_KEY_STORE_WRITE_AREA
            KeyStoreWrittenArea = 0x404, // AES_KEY_STORE_WRITTEN_AREA
            KeyStoreSize = 0x408, // AES_KEY_STORE_SIZE
            KeyStoreReadArea = 0x40C, // AES_KEY_STORE_READ_AREA
            AesInputVector = 0x540, // AES_AES_IV_0
            AesControl = 0x550, // AES_AES_CTRL
            AesCryptoLength0 = 0x554, // AES_AES_C_LENGTH_0
            AesCryptoLength1 = 0x558, // AES_AES_C_LENGTH_1
            AesAuthLength = 0x55C, // AES_AES_AUTH_LENGTH
            AlgorithmSelection = 0x700, // AES_CTRL_ALG_SEL
            InterruptConfiguration = 0x780, // AES_CTRL_INT_CFG
            InterruptEnable = 0x784, // AES_CTRL_INT_EN
            InterruptClear = 0x788, // AES_CTRL_INT_CLR
            InterruptStatus = 0x790, // AES_CTRL_INT_STAT
        }

        private enum DmaDestination
        {
            KeyStore = 1,
            Aes = 2,
            HashEngine = 4
        }

        private enum KeySize
        {
            Bits128 = 1,
            Bits192 = 2,
            Bits256 = 3
        }

        private enum Direction
        {
            Decryption,
            Encryption
        }
    }
}
