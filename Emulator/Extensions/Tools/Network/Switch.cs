//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Core.Structure;
using Emul8.Peripherals.Network;
using System.Linq;
using System.Collections.Concurrent;
using Emul8.Network;
using Emul8.Logging;
using System.Collections.Generic;
using Emul8.Exceptions;
using Emul8.Peripherals;
using Emul8.Time;

namespace Emul8.Tools.Network
{
    public static class SwitchExtensions
    {
        public static void CreateSwitch(this Emulation emulation, string name)
        {
            emulation.ExternalsManager.AddExternal(new Switch(), name);
        }
    }

    public class Switch : SynchronizedExternalBase, IExternal, IHasOwnLife, IConnectable<IMACInterface>
    {
        public void AttachTo(IMACInterface iface)
        {
            ifaces[iface.MAC] = iface;
            ifacesDelegates[iface] = (s, f) => ForwardToReceiver(f, iface);
            iface.Link.TransmitFromParentInterface += ifacesDelegates[iface];
        }

        public void DetachFrom(IMACInterface iface)
        {
            IMACInterface value;
            ifaces.TryRemove(iface.MAC, out value);

            Action<NetworkLink, EthernetFrame> deleg;
            if (ifacesDelegates.TryRemove(iface, out deleg))
            {
                iface.Link.TransmitFromParentInterface -= deleg;
            }
        }

        public void EnablePromiscuousMode(IMACInterface iface)
        {
            lock(promiscuousMode)
            {
                if(!ifaces.Values.Contains(iface))
                {
                    throw new RecoverableException("The interface is not register, you must connect it in order to set promiscuous mode");
                }
                promiscuousMode.Add(iface);
            }
        }

        public void DisablePromiscuousMode(IMACInterface iface)
        {
            lock(promiscuousMode)
            {
                if(!promiscuousMode.Contains(iface))
                {
                    throw new RecoverableException("The interface is not in promiscuous mode");
                }
                promiscuousMode.Remove(iface);
            }
        }

        public void Start()
        {
            Resume();
        }

        public void Pause()
        {
            started = false;
        }

        public void Resume()
        {
            started = true;
        }

        private void ForwardToReceiver(EthernetFrame frame, IMACInterface sender)
        {
            if(!frame.DestinationMAC.HasValue)
            {
                this.Log(LogLevel.Warning, "Destination MAC not set, the frame has unsupported format.");
                return;
            }
            ExecuteOnNearestSync(() =>
            {
                if(!started)
                {
                    return;
                }
                var destination = frame.DestinationMAC.Value;
                IMACInterface destIface;
                if(ifaces.TryGetValue(destination, out destIface))
                {
                    foreach(var promiscuousIface in promiscuousMode)
                    {
                        if(promiscuousIface != destIface)
                        {
                            promiscuousIface.Link.ReceiveFrameOnInterface(frame);
                        }
                    }
                    destIface.Link.ReceiveFrameOnInterface(frame);
                }
                else
                {
                    foreach(var other in ifaces.Values.Distinct().Where(x=>x != sender))
                    {
                        other.Link.ReceiveFrameOnInterface(frame);
                    }
                }
            });

            // at the same we will potentially add current MAC address assigned to the source
            if(!frame.SourceMAC.HasValue)
            {
                this.Log(LogLevel.Warning, "Source MAC not set, cannot update switch cache.");
                return;
            }
            var source = frame.SourceMAC.Value;
            ifaces[source] = sender;
        }

        private bool started;
        private readonly List<IMACInterface> promiscuousMode = new List<IMACInterface>();
        private readonly ConcurrentDictionary<MACAddress, IMACInterface> ifaces = new ConcurrentDictionary<MACAddress, IMACInterface>();
        private readonly ConcurrentDictionary<IMACInterface, Action<NetworkLink, EthernetFrame>> ifacesDelegates = new ConcurrentDictionary<IMACInterface, Action<NetworkLink, EthernetFrame>>();
    }
}

