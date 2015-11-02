//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Collections.Generic;
using System.Linq;
using Emul8.Time;
using System.Collections.Concurrent;

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
        public void AttachTo(IRadio radio)
        {
            Action<byte[]> handler = x => HandleFrameSent(x, radio);
            radios.AddOrUpdate(radio, handler, (x, y) => handler);
            radio.FrameSent += handler;
        }

        private void HandleFrameSent (byte[] obj, IRadio sender)
        {
            ExecuteOnNearestSync(() =>
            {
                foreach(var item in radios.Keys.Where(x => x != sender))
                {
                    item.ReceiveFrame(obj);
                }
            });
        }

        public void DetachFrom(IRadio radio)
        {
            Action<byte[]> action;
            radios.TryRemove(radio, out action);
            radio.FrameSent -= action;
        }

        private readonly ConcurrentDictionary<IRadio, Action<byte[]>> radios = new ConcurrentDictionary<IRadio, Action<byte[]>>();
    }
}

