//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Linq;
using Emul8.Core.Structure;
using System.Net;
using PacketDotNet;
using Emul8.Peripherals.Network;

namespace Emul8.Network
{
    public class EthernetFrame
    {
        public EthernetFrame(byte[] data, bool externalSource = false)
        {
            packet = Packet.ParsePacket(LinkLayers.Ethernet, data);
            IsFromHost = externalSource;
        }

        public byte[] Bytes
        {
            get
            {
                return packet.Bytes.ToArray();
            }
        }

        public int Length
        {
            get
            {
                return packet.BytesHighPerformance.Length;
            }
        }

        public override string ToString()
        {
            return packet.ToString();
        }

        public MACAddress? SourceMAC
        {
            get
            {
                var ether = (EthernetPacket)packet.Extract(typeof(EthernetPacket));
                return ether != null ? (MACAddress?)ether.SourceHwAddress : null;
            }
        }

        public MACAddress? DestinationMAC
        {
            get
            {
                var ether = (EthernetPacket)packet.Extract(typeof(EthernetPacket));
                return ether != null ? (MACAddress?)ether.DestinationHwAddress : null;
            }
        }

        public IPAddress SourceIP
        {
            get
            {
                var ip = (IpPacket)packet.Extract(typeof(IpPacket));
                return ip != null ? ip.SourceAddress : null;
            }
        }

        public IPAddress DestinationIP
        {
            get
            {
                var ip = (IpPacket)packet.Extract(typeof(IpPacket));
                return ip != null ? ip.DestinationAddress : null;
            }
        }

        public void FillWithChecksums(EtherType[] supportedEtherTypes, IPProtocolType[] supportedIpProtocolTypes)
        {
            var packetNetIpProtocols = supportedIpProtocolTypes.Select(x => (PacketDotNet.IPProtocolType)x).ToArray();
            var packetNetEtherTypes = supportedEtherTypes.Select(x => (EthernetPacketType)x).ToArray();
            packet.RecursivelyUpdateCalculatedValues(packetNetEtherTypes, packetNetIpProtocols);
        }

        public bool IsFromHost
        { 
            get;
            private set; 
        }

        private readonly Packet packet;
    }
}