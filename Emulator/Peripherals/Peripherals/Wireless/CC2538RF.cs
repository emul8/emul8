//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.Bus.Wrappers;
using Emul8.Logging;
using System;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Utilities;
using Antmicro.Migrant;

namespace Emul8.Peripherals.Wireless
{
    public class CC2538RF : IDoubleWordPeripheral, IBytePeripheral, IKnownSize, IRadio
    {
        public CC2538RF()
        {
            random = new Random();
            IRQ = new GPIO();
            Reset();
        }

        public uint ReadDoubleWord(long offset)
        {
            if(offset < 0x200)
            {
                //RX FIFO memory area
                this.LogUnhandledRead(offset);
                return 0;
            }
            if(offset < 0x400)
            {
                //TX FIFO memory area
                this.LogUnhandledRead(offset);
                return 0;
            }
            if(offset >= 0x400 && offset <= 0x57C)
            {
                //Source address table, page 663
                this.LogUnhandledRead(offset);
                return 0;
            }
            if(offset >= 0x580 && offset <= 0x5D4)
            {
                return ReadFFSM(offset);
            }
            if(offset <= 0x5D8 && offset <= 0x5E2)
            {
                //Temporary storage
                this.LogUnhandledRead(offset);
                return 0;
            }
            if(offset >= 0x600 && offset <= 0x7E8)
            {
                return ReadXREG(offset);
            }
            if(offset >= 0x828 && offset <= 0x838)
            {
                return ReadSFR(offset);
            }
            this.LogUnhandledRead(offset);
            return 0;

        }


        public void WriteDoubleWord(long offset, uint value)
        {
            if(offset < 0x200)
            {
                //RX FIFO memory area
                return;
            }
            if(offset < 0x400)
            {
                //TX FIFO memory area
                return;
            }
            if(offset >= 0x400 && offset <= 0x57C)
            {
                //Source address table, page 663
                this.LogUnhandledWrite(offset, value);            
                return;
            }
            if(offset >= 0x580 && offset <= 0x5D4)
            {
                WriteFFSM(offset, value);
                return;
            }
            if(offset <= 0x5D8 && offset <= 0x5E2)
            {
                //Temporary storage
                this.LogUnhandledWrite(offset, value);
                return;
            }
            if(offset >= 0x600 && offset <= 0x7E8)
            {
                WriteXREG(offset, value);
                return;
            }
            if(offset >= 0x828 && offset <= 0x838)
            {
                WriteSFR(offset, value);
                return;
            }
            this.LogUnhandledWrite(offset, value);
        }

        //used by uDMA
        public byte ReadByte(long offset)
        {
            if(offset == (long)SFRRegisters.RFData)
            {
                return DequeueData();
            }
            this.Log(LogLevel.Warning, "{0} does not implement byte reads apart from {1} (0x{2:X}).", this.GetType().Name, SFRRegisters.RFData.ToString(), (long)SFRRegisters.RFData);
            this.LogUnhandledRead(offset);
            return 0;
        }

        //used by uDMA
        public void WriteByte(long offset, byte value)
        {
            if(offset == (long)SFRRegisters.RFData)
            {
                EnqueueData(value);
                return;
            }
            this.Log(LogLevel.Warning, "{0} does not implement byte writes apart from {1} (0x{2:X}).", this.GetType().Name, SFRRegisters.RFData.ToString(), (long)SFRRegisters.RFData);
            this.LogUnhandledWrite(offset, value);
        }

        public long Size
        {
            get
            {
                return 0x1000;
            }
        }

        public void Reset()
        {
            lastReceivedFrame = null;
            txPendingCounter = 0;
            rxEnableMask = 0;
            this.Trace("rxqueue");
            rxQueue.Clear();
        }

        public event Action<byte[]> FrameSent;

        public void ReceiveFrame(byte[] frame)
        {
            lock(rxLock)
            {
                this.DebugLog("Receiving frame {0}.", frame.Select(x => "0x{0:X}".FormatWith(x)).Stringify());

                var frameToQueue = frame.Take(frame.Length - 2) //without crc
                .Concat(new byte[]{ 70, (byte)(1u << 7 | 100) }); //CRC ok (maybe TODO calculation); correlation value 100 means near maximum quality.

                rxQueue.Enqueue(new Queue<byte>(frameToQueue));
           
                if((lastReceivedFrame == null || lastReceivedFrame.Count == 0)) //no frame pending
                {
                    IRQ.Set();
                }
            }
        }
            
        public GPIO IRQ { get; private set; }
        //todo: errirq

        #region FFSM

        private uint ReadFFSM(long offset)
        {
            switch((FFSMRegisters)offset)
            {
            case FFSMRegisters.ExtAddress0:
            case FFSMRegisters.ExtAddress1:
            case FFSMRegisters.ExtAddress2:
            case FFSMRegisters.ExtAddress3:
            case FFSMRegisters.ExtAddress4:
            case FFSMRegisters.ExtAddress5:
            case FFSMRegisters.ExtAddress6:
            case FFSMRegisters.ExtAddress7:
                return localAddress[(offset - (int)FFSMRegisters.ExtAddress0) / 4];
            case FFSMRegisters.PANId0:
                return panId0;
            case FFSMRegisters.PANId1:
                return panId1;
            case FFSMRegisters.ShortAddress0:
                return shortAddress0;
            case FFSMRegisters.ShortAddress1:
                return shortAddress1;
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }

        }

        private void WriteFFSM(long offset, uint value)
        {
            switch((FFSMRegisters)offset)
            {
            case FFSMRegisters.ExtAddress0:
            case FFSMRegisters.ExtAddress1:
            case FFSMRegisters.ExtAddress2:
            case FFSMRegisters.ExtAddress3:
            case FFSMRegisters.ExtAddress4:
            case FFSMRegisters.ExtAddress5:
            case FFSMRegisters.ExtAddress6:
            case FFSMRegisters.ExtAddress7:
                localAddress[(offset - (int)FFSMRegisters.ExtAddress0) / 4] = (byte)value;
                break;
            case FFSMRegisters.PANId0:
                panId0 = value & 0xFF;
                break;
            case FFSMRegisters.PANId1:
                panId1 = value & 0xFF;
                break;
            case FFSMRegisters.ShortAddress0:
                shortAddress0 = value & 0xFF;
                break;
            case FFSMRegisters.ShortAddress1:
                shortAddress1 = value & 0xFF;
                break;
            default:
                this.LogUnhandledWrite(offset, value);
                break;
            }
        }

        [RegisterMapper.RegistersDescription]
        private enum FFSMRegisters
        {
            ExtAddress0 = 0x5A8,
            ExtAddress1 = 0x5AC,
            ExtAddress2 = 0x5B0,
            ExtAddress3 = 0x5B4,
            ExtAddress4 = 0x5B8,
            ExtAddress5 = 0x5BC,
            ExtAddress6 = 0x5C0,
            ExtAddress7 = 0x5C4,
            PANId0 = 0x5C8,
            PANId1 = 0x5CC,
            ShortAddress0 = 0x5D0,
            ShortAddress1 = 0x5D1
        }

        //reset undefined, so not neccesary
        private byte[] localAddress = new byte[8];
        private uint panId0;
        private uint panId1;
        private uint shortAddress0;
        private uint shortAddress1;

        #endregion

        #region XREG
        private uint ReadXREG(long offset)
        {
            switch((XREGRegisters)offset)
            {
            case XREGRegisters.FrameHandling0:
                return frameHandling;
            case XREGRegisters.RxEnabled:
                return rxEnableMask;
            case XREGRegisters.RadioStatus0:
                return 1u << 7 | 1u; //frequency synthesis calibration has been performed AND non-zero state, because contiki driver needs it :|
            case XREGRegisters.RadioStatus1:
                var txActive = 0u;
                if(txPendingCounter > 0)
                {
                    txActive = 1u << 1;
                    txPendingCounter--;

                }
                lock(rxLock)
                {
                    return ((lastReceivedFrame == null || lastReceivedFrame.Count == 0) && rxQueue.Count == 0
                    ? 0u : ((3 << 6) | (1 << 4) | 1))//FIFO, FIFOP, SFD and RX_ACTIVE bits if rxfifo not empty.
                    | 1u << 4//clear channel assessment
                    | txActive; //HACK! TX_ACTIVE is required to be set as 1 few times in a row for contiki
                }
            case XREGRegisters.RSSIValidStatus:
                return 1;
            case XREGRegisters.InterruptMask0:
                return interruptMask0;
            case XREGRegisters.InterruptMask1:
                return interruptMask1;
            case XREGRegisters.RandomData:
                return (uint)(random.Next() & 3);
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }

        }

        private void WriteXREG(long offset, uint value)
        {
            switch((XREGRegisters)offset)
            {
            case XREGRegisters.FrameHandling0:
                frameHandling = (byte)value;
                break;
            case XREGRegisters.InterruptMask0:
                interruptMask0 = (byte)value;
                break;
            case XREGRegisters.InterruptMask1:
                interruptMask1 = (byte)value;
                break;
            default:
                this.LogUnhandledWrite(offset, value);
                break;
            }
        }

        [RegisterMapper.RegistersDescription]
        private enum XREGRegisters
        {
            FrameHandling0 = 0x624,
            RxEnabled = 0x62C,
            RadioStatus0 = 0x648,
            RadioStatus1 = 0x64C,
            RSSIValidStatus = 0x664,
            InterruptMask0 =  0x68C,
            InterruptMask1 = 0x690,
            RandomData = 0x69C,
        }

        private byte interruptMask0;
        private byte interruptMask1;
        private byte frameHandling;
        #endregion

        #region SFR
        private uint ReadSFR(long offset)
        {
            switch((SFRRegisters)offset)
            {
            case SFRRegisters.RFData:
                return DequeueData();
            default:
                this.LogUnhandledRead(offset);
                return 0;
            }

        }

        private void WriteSFR(long offset, uint value)
        {
            switch((SFRRegisters)offset)
            {
            case SFRRegisters.RFData:
                EnqueueData((byte)value);
                break;
            case SFRRegisters.InterruptFlags0:
                IRQ.Unset();
                break;
            case SFRRegisters.CommandStrobeProcessor:
                HandleSFRInstruction(value);
                break;
            default:
                this.LogUnhandledWrite(offset, value);
                break;
            }
        }

        private void HandleSFRInstruction(uint value)
        {
            switch((CSPInstructions)value)
            {
            case CSPInstructions.RxOn:
                rxEnableMask |= 1u << 7;
                break;
            case CSPInstructions.TxOn:
                txPendingCounter = 4;
                SendData();
                break;
            case CSPInstructions.RxFifoFlush:
                if(rxQueue.Count != 0)
                {
                    this.Log(LogLevel.Warning, "Dropping unreceived frame.");
                }
                lastReceivedFrame = null;

                this.Trace("rxqueue");
                rxQueue.Clear();
                break;
            case CSPInstructions.TxFifoFlush:
                txQueue.Clear();
                break;
            case CSPInstructions.RFOff:
                rxEnableMask = 0;
                break;
            default:
                this.Log(LogLevel.Error, "Unsupported CSP instruction {0}.", value);
                break;
            }
        }

        [RegisterMapper.RegistersDescription]
        private enum SFRRegisters
        {
            RFData = 0x828,
            InterruptFlags0 = 0x834,
            CommandStrobeProcessor = 0x838
        }

        private enum CSPInstructions
        {
            RxOn = 0xE3,
            TxOn = 0xE9,
            RxFifoFlush = 0xED,
            TxFifoFlush = 0xEE,
            RFOff = 0xEF
        }
        #endregion

        private byte DequeueData()
        {
            lock(rxLock)
            {
                if(lastReceivedFrame == null || lastReceivedFrame.Count == 0)
                {
                    if(rxQueue.Count > 0)
                    {
                        this.Trace("rxqueue");
                        lastReceivedFrame = rxQueue.Dequeue();
                        return lastReceivedFrame.Dequeue();
                    }
                    return 0;
                }
                else
                {
                    return lastReceivedFrame.Dequeue();
                }
            }
        }

        private void EnqueueData(byte value)
        {
            this.DebugLog("Enqueuing data: 0x{0:X}", value);
            txQueue.Enqueue((byte)(value & 0xFF));
        }

        private void SendData()
        {
            var lastFrame = txQueue.ToList().Concat(new byte[]{0,0}).ToArray();
            txQueue.Clear();
            this.DebugLog("Sending frame {0}.", lastFrame.Select(x => "0x{0:X}".FormatWith(x)).Stringify());
            var frameSent = FrameSent;
            if(frameSent != null)
            {
                frameSent(lastFrame);
            }
        }

        private readonly object rxLock = new object();
        private int txPendingCounter;
        private uint rxEnableMask;
        private Queue<byte> lastReceivedFrame;
	    private readonly Queue<Queue<byte>> rxQueue = new Queue<Queue<byte>>();
        private readonly Queue<byte> txQueue = new Queue<byte>();
        [Constructor]
        private readonly Random random;
    }
}

