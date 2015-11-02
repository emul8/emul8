//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Peripherals.Memory;

namespace Emul8.Peripherals.DMA
{
    public sealed class DmaEngine
    {
        public DmaEngine(Machine machine)
        {
            this.machine = machine;
        }

        public void IssueCopy(Request request)
        {
            // some sanity checks
            if((request.Size % (int)request.ReadTransferType) != 0 || (request.Size % (int)request.WriteTransferType) != 0)
            {
                throw new ArgumentException("Request size is not aligned properly to given read or write transfer type (or both).");
            }

            var sysbus = machine.SystemBus;
            var buffer = new byte[request.Size];
            IBusRegistered<IBusPeripheral> whatIsAt;
            if(!request.Source.Address.HasValue)
            {
                Array.Copy(request.Source.Array, request.Source.StartIndex.Value, buffer, 0, request.Size);
            }
            else
            {
                var sourceAddress = request.Source.Address.Value;
                whatIsAt = sysbus.WhatIsAt(sourceAddress);
                //allow ReadBytes if the read memory is without gaps
                if((whatIsAt == null || whatIsAt.Peripheral is MappedMemory) && (int)request.ReadTransferType == request.SourceIncrementStep)
                {
                    if(request.IncrementReadAddress)
                    {
                        sysbus.ReadBytes(sourceAddress, request.Size, buffer, 0);
                    }
                    else
                    {
                        sysbus.ReadBytes(sourceAddress, (int)request.ReadTransferType, buffer, 0);
                    }
                }
                else if(whatIsAt != null)
                {
                    var transferred = 0;
                    var offset = 0;
                    while(transferred < request.Size)
                    {
                        switch(request.ReadTransferType)
                        {
                        case TransferType.Byte:
                            buffer[transferred] = sysbus.ReadByte(sourceAddress + offset);
                            break;
                        case TransferType.Word:
                            BitConverter.GetBytes(sysbus.ReadWord(sourceAddress + offset)).CopyTo(buffer, transferred);
                            break;
                        case TransferType.DoubleWord:
                            BitConverter.GetBytes(sysbus.ReadDoubleWord(sourceAddress + offset)).CopyTo(buffer, transferred);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                        }
                        transferred += (int)request.ReadTransferType;
                        if(request.IncrementReadAddress)
                        {
                            offset += request.SourceIncrementStep;
                        }
                    }
                }
            }

            if(!request.Destination.Address.HasValue)
            {
                Array.Copy(buffer, 0, request.Destination.Array, request.Destination.StartIndex.Value, request.Size);
            }
            else
            {
                var destinationAddress = request.Destination.Address.Value;
                whatIsAt = sysbus.WhatIsAt(destinationAddress);
                if((whatIsAt == null || whatIsAt.Peripheral is MappedMemory) && (int)request.WriteTransferType == request.DestinationIncrementStep)
                {
                    if(request.IncrementWriteAddress)
                    {
                        sysbus.WriteBytes(buffer, destinationAddress);
                    }
                    else
                    {
                        // if the place to write is memory and we're not incrementing address, effectively only the last byte is written
                        sysbus.WriteByte(destinationAddress, buffer[buffer.Length - 1]);
                    }
                }
                else
                {
                    var transferred = 0;
                    var offset = 0;
                    while(transferred < request.Size)
                    {
                        switch(request.WriteTransferType)
                        {
                        case TransferType.Byte:
                            sysbus.WriteByte(destinationAddress + offset, buffer[transferred]);
                            break;
                        case TransferType.Word:
                            sysbus.WriteWord(destinationAddress + offset, BitConverter.ToUInt16(buffer, transferred));
                            break;
                        case TransferType.DoubleWord:
                            sysbus.WriteDoubleWord(destinationAddress + offset, BitConverter.ToUInt32(buffer, transferred));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                        }
                        transferred += (int)request.WriteTransferType;
                        if(request.IncrementWriteAddress)
                        {
                            offset += request.DestinationIncrementStep;
                        }
                    }
                }
            }
        }

        private readonly Machine machine;
    }
}

