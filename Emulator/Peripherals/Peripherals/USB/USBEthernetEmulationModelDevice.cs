//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Network;
using System.Collections.Generic;
using Emul8.Network;
using Emul8.Core.Structure;

namespace Emul8.Peripherals.USB 
{
    public class USBEthernetEmulationModelDevice : IUSBPeripheral, IMACInterface
    {
        public event Action <uint> SendInterrupt
        {
            add {}
            remove {}
        }
        public event Action <uint> SendPacket
        {
            add {}
            remove {}
        }

        public USBEthernetEmulationModelDevice ()
        {
            Link = new NetworkLink(this);
        }

        public void Reset()
        {
        }

        public USBDeviceSpeed GetSpeed()
        {
            return USBDeviceSpeed.Low;
        }
        
        #region IUSBDevice implementation
        public byte[] ProcessClassGet(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new System.NotImplementedException ();
        }
            public byte[] GetDataBulk(USBPacket packet)
        {
            return null;
        }
                public void WriteDataBulk(USBPacket packet)
        {
            
        }
                public uint GetAddress()
        {
            return 0;
        }
        public void WriteDataControl(USBPacket packet)
        {
        }
                      public     byte GetTransferStatus()
        {
        return 0;
        } 
        public byte[] GetDataControl(USBPacket packet)
        {
            return null;
        }
                public byte[] GetDescriptor(USBPacket packet, USBSetupPacket setupPacket)
        {
            return null;
        }
        public void ProcessClassSet(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new System.NotImplementedException ();
        }
                public byte[] WriteInterrupt(USBPacket packet)
        {
            return null;
        }
        public void SetDataToggle(byte endpointNumber)
        {
            throw new NotImplementedException ();    
        }
        
        public void CleanDataToggle(byte endpointNumber)
        {
            throw new NotImplementedException ();
        }
        
        public void ToggleDataToggle(byte endpointNumber)
        {
            throw new NotImplementedException ();    
        }
        
        public bool GetDataToggle(byte endpointNumber)
        {
            throw new NotImplementedException ();
        }
        
        public void ClearFeature (USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public byte[] GetConfiguration ()
        {
            throw new NotImplementedException ();
        }



        public byte[] GetInterface(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public byte[] GetStatus(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public void SetAddress (uint address)
        {
            throw new NotImplementedException ();
        }

        public void SetConfiguration(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public void SetDescriptor (USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public void SetFeature(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public void SetInterface (USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new NotImplementedException ();
        }

        public void SyncFrame (uint endpointId)
        {
            throw new NotImplementedException ();
        }

        public void WriteData (byte[] data)
        {
            throw new NotImplementedException ();
        }

        public byte[] GetData ()
        {
            throw new NotImplementedException ();
        }

        public void ReceiveFrame (EthernetFrame frame)
        {
            throw new NotImplementedException ();
        }

        public NetworkLink Link { get; private set; }
        public MACAddress MAC { get; set; }
        private const ushort EnglishLangId = 0x09;


        #endregion
        
        
        private class ethernetControllerSetting
        {
            public long MACAddress = 0xFFFFFFFFFFFF;
            public byte fullSpeedPollingInterval = 0x01;
            public byte hiSpeedPollingInterval = 0x04;
            public byte configurationFlag = 0x05;
        }

        public byte[] ProcessVendorGet(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessVendorSet(USBPacket packet, USBSetupPacket setupPacket)
        {
            throw new System.NotImplementedException();
        }
    }
}

