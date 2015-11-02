//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using System.Linq;
using Emul8.Time;
using System.Collections.Concurrent;

namespace Emul8.Peripherals.UART
{
    public static class UARTHubExtensions 
    {
        public static void CreateUARTHub(this Emulation emulation, string name)
        {
            emulation.ExternalsManager.AddExternal(new UARTHub(), name);
        }
    }

    public sealed class UARTHub : SynchronizedExternalBase, IExternal, IHasOwnLife, IConnectable<IUART>
    {
        public void AttachTo(IUART uart)
        {
            uarts.Add(uart);
            uart.CharReceived += x => HandleCharReceived(x, uart);
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

        private void HandleCharReceived (byte obj, IUART sender)
        {
            if(!started)
            {
                return;
            }
            ExecuteOnNearestSync(() =>
            {
                foreach(var item in uarts.Where(x=> x!= sender))
                {
                    item.WriteChar(obj);
                }
            });
        }

        public void DetachFrom(IUART uart)
        {
            throw new NotImplementedException();
        }

        private bool started;
        private ConcurrentBag<IUART> uarts = new ConcurrentBag<IUART>();
    }
}

