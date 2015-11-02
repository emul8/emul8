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

namespace Emul8.Peripherals.USB 
{
    public class USBEthernetEmulationModelDevice : IUSBPeripheral, INetworkInterface
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
        
 /*       private ConfigurationUSBDescriptor configurationDescriptor = new ConfigurationUSBDescriptor()
        {
            TotalLength = 10, //FIXME: proper values
            NumberOfInterfaces = 1,
            ConfigurationValue = 0,
            ConfigurationIndex = 3,
            SelfPowered = true,
            RemoteWakeup = false,
            MaxPower = 0x01
        };

        private ConfigurationUSBDescriptor otherConfigurationDescriptor = new ConfigurationUSBDescriptor();
        private StringUSBDescriptor stringDescriptor = null;

        private StandardUSBDescriptor deviceDescriptor = new StandardUSBDescriptor
        {
            DeviceClass=0x02, //communication device class
            DeviceSubClass = 0x0c,//communication device subclass/EEM
            USB = 0x0200,
            DeviceProtocol = 0x07,//EEM
            MaxPacketSize = 64,
            VendorId = 0x0424,
            ProductId = 0xec00,
            Device = 0x0100,
            ManufacturerIndex = 4,
            ProductIndex = 1,
            SerialNumberIndex = 2,
            NumberOfConfigurations = 1
        };

        private DeviceQualifierUSBDescriptor deviceQualifierDescriptor = new DeviceQualifierUSBDescriptor()
        {
            USB = 0x0200,
            DeviceClass = 0x02,
            DeviceSubClass = 0x0c,
            DeviceProtocol = 0x07,
            MaxPacketSize = 64,
            NumberOfConfigurations = 1
        };

        private EndpointUSBDescriptor endpointDescriptor = new EndpointUSBDescriptor()
        {
            EndpointNumber = 1,
            InEnpoint = true,
            TransferType = EndpointUSBDescriptor.TransferTypeEnum.Control,//FIXME: proper values
            SynchronizationType = 0,
            UsageType = 0,
            MaxPacketSize = 64,
            Interval = 1
        
        };

        private InterfaceUSBDescriptor interfaceDescriptor = new InterfaceUSBDescriptor()
        {
            InterfaceNumber = 0,
            AlternateSetting = 0,
            NumberOfEndpoints = 3,
            InterfaceClass = 0x02,
            InterfaceSubClass = 0x0c,
            InterfaceProtocol = 0x07,
            InterfaceIndex = 0
        };
        */



        private const ushort EnglishLangId = 0x09;

   /*     private uint address;

        private Dictionary<ushort, string[]> stringValues = new Dictionary<ushort, string[]>()
        {
            {EnglishLangId, new string[]{
                    "",
                    "Our Product",
                    "0xALLMAN",
                    "Configuration",
                    "AntMicro"
                }}
        };
       */
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

