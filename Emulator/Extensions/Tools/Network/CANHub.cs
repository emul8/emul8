//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals;
using System.Collections.Generic;
using Emul8.Peripherals.CAN;
using Emul8.Time;

namespace Emul8.Tools.Network
{
    public static class CANHubExtensions
    {
        public static void CreateCANHub(this Emulation emulation, string name)
        {
            emulation.ExternalsManager.AddExternal(new CANHub(), name);
        }
    }

    public sealed class CANHub : SynchronizedExternalBase, IExternal, IHasOwnLife, IConnectable<ICAN>
    {
        public CANHub()
        {
            sync = new object();
            attached = new List<ICAN>();
            handlers = new Dictionary<ICAN, Action<int, byte[]>>();
        }

        public void AttachTo(ICAN iface)
        {
            lock(sync)
            {
                attached.Add(iface);
                handlers.Add(iface, (id, data) => Transmit(iface, id, data));
                iface.FrameSent += handlers[iface];
            }
        }

        public void DetachFrom(ICAN iface)
        {
            lock(sync)
            {
                attached.Remove(iface);
                iface.FrameSent -= handlers[iface];
                handlers.Remove(iface);
            }
        }

            
        public void Start()
        {
            Resume();
        }

        public void Pause()
        {
            lock(sync)
            {
                started = false;
            }
        }

        public void Resume()
        {
            lock(sync)
            {
                started = true;
            }
        }

        private void Transmit(ICAN sender, int id, byte[] data)
        {
            ExecuteOnNearestSync(() =>
            {
                lock(sync)
                {
                    if(!started)
                    {
                        return;
                    }
                    foreach(var iface in attached)
                    {
                        if(iface == sender)
                        {
                            continue;
                        }
                        iface.OnFrameReceived(id, data);
                    }
                }
            });
        }

        private readonly List<ICAN> attached;
        private readonly Dictionary<ICAN, Action<int, byte[]>> handlers;
        private bool started;
        private object sync;
    }
}

