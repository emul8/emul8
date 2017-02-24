//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Collections.Generic;
using System.Linq;
using Emul8.Time;
using Emul8.Logging;

namespace Emul8.Peripherals.Wireless
{
    public static class WirelessExtensions
    {
        public static void CreateWirelessMedium(this Emulation emulation, string name)
        {
            emulation.ExternalsManager.AddExternal(new WirelessMedium(), name);
        }
    }

    public sealed class WirelessMedium : SynchronizedExternalBase, IExternal, IConnectable<IRadio>
    {
        public WirelessMedium()
        {
            packetsToSend = new Queue<PacketWithSender>();
            radios = new Dictionary<IRadio, Position>();
            mediumFunction = SimpleMediumFunction.Instance;
        }

        public void AttachTo(IRadio radio)
        {
            radios.Add(radio, new Position());
            radio.FrameSent += FrameSentHandler;
        }

        public void DetachFrom(IRadio radio)
        {
            radios.Remove(radio);
            radio.FrameSent -= FrameSentHandler;
        }

        public void SetMediumFunction(IMediumFunction function)
        {
            mediumFunction = function;
        }

        public void SetPosition(IRadio radio, decimal x, decimal y, decimal z)
        {
            radios[radio] = new Position(x, y, z);
        }

        public event Action<IRadio, IRadio, byte[]> FrameTransmitted;

        private void FrameSentHandler(IRadio sender, byte[] packet)
        {
            lock(packetsToSend)
            {
                packetsToSend.Enqueue(new PacketWithSender(packet, sender.Channel, sender, radios[sender]));
                if(executeOnSyncAlreadyQueued)
                {
                    return;
                }
                ExecuteOnNearestSync(NearestSyncHandler);
                executeOnSyncAlreadyQueued = true;
            }
        }

        private void NearestSyncHandler()
        {
            var frameTransmitted = FrameTransmitted;
            lock(packetsToSend)
            {
                executeOnSyncAlreadyQueued = false;
                while(packetsToSend.Count > 0)
                {
                    var packet = packetsToSend.Dequeue();
                    foreach(var radioAndPosition in radios.Where(x => x.Key != packet.Sender))
                    {
                        var receiver = radioAndPosition.Key;
                        var receiverPosition = radioAndPosition.Value;
                        var sender = packet.Sender;
                        var senderPosition = packet.SenderPosition;

                        var senderName = sender.ToString();
                        var receiverName = receiver.ToString();
                        var currentEmulation = EmulationManager.Instance.CurrentEmulation;
                        currentEmulation.TryGetEmulationElementName(sender, out senderName);
                        currentEmulation.TryGetEmulationElementName(receiver, out receiverName);

                        if(mediumFunction.CanReach(senderPosition, receiverPosition) && receiver.Channel == packet.Channel)
                        {
                            this.NoisyLog("Packet {0} -> {1} delivered, size {2}.", senderName, receiverName, packet.Frame.Length);
                            receiver.ReceiveFrame(packet.Frame);
                            if(frameTransmitted != null)
                            {
                                frameTransmitted(sender, receiver, packet.Frame);
                            }
                        }
                        else
                        {
                            this.NoisyLog("Packet {0} -> {1} NOT delivered, size {2}.", senderName, receiverName, packet.Frame.Length);
                        }
                    }
                }
            }
        }

        private bool executeOnSyncAlreadyQueued;
        private IMediumFunction mediumFunction;
        private readonly Dictionary<IRadio, Position> radios;
        private readonly Queue<PacketWithSender> packetsToSend;

        private sealed class PacketWithSender
        {
            public PacketWithSender(byte[] frame, int channel, IRadio sender, Position senderPosition)
            {
                Frame = frame;
                Channel = channel;
                Sender = sender;
                SenderPosition = senderPosition;
            }

            public int Channel { get; private set; }
            public byte[] Frame { get; private set; }
            public IRadio Sender { get; private set; }
            public Position SenderPosition { get; private set; }
        }
    }
}

