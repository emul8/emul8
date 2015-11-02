//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.DMA
{
    public struct Request
    {
        public Request(Place source, Place destination, int size, TransferType readTransferType, TransferType writeTransferType,
            bool incrementReadAddress = true, bool incrementWriteAddress = true) : this()
        {
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.ReadTransferType = readTransferType;
            this.WriteTransferType = writeTransferType;
            this.IncrementReadAddress = incrementReadAddress;
            this.IncrementWriteAddress = incrementWriteAddress;
            this.SourceIncrementStep = (int)readTransferType;
            this.DestinationIncrementStep = (int)writeTransferType;
        }

        public Request(Place source, Place destination, int size, TransferType readTransferType, TransferType writeTransferType, 
            int sourceIncrementStep, int destinationIncrementStep, bool incrementReadAddress = true, 
            bool incrementWriteAddress = true) : this()
        {
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.ReadTransferType = readTransferType;
            this.WriteTransferType = writeTransferType;
            this.IncrementReadAddress = incrementReadAddress;
            this.IncrementWriteAddress = incrementWriteAddress;
            this.SourceIncrementStep = sourceIncrementStep;
            this.DestinationIncrementStep = destinationIncrementStep;
        }

        public Place Source { get; private set; }
        public Place Destination { get; private set; }
        public int SourceIncrementStep { get; private set; }
        public int DestinationIncrementStep { get; private set; }
        public int Size { get; private set; }
        public TransferType ReadTransferType { get; private set; }
        public TransferType WriteTransferType { get; private set; }
        public bool IncrementReadAddress { get; private set; }
        public bool IncrementWriteAddress { get; private set; }
    }
}

