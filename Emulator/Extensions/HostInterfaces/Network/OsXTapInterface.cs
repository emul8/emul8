//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals.Network;
using Emul8.Peripherals;
using Emul8.Core;
using System.IO;
using System.Threading;
using Emul8.Logging;
using System.Threading.Tasks;
using Emul8.Core.Structure;
using Emul8.Network;
using System.Net.NetworkInformation;
using System.Linq;
using Emul8.Utilities;

namespace Emul8.HostInterfaces.Network
{
    public sealed class OsXTapInterface : ITapInterface, IHasOwnLife, IDisposable
    {
        public OsXTapInterface(string device)
        {
            deviceFile = File.Open(device, FileMode.Open, FileAccess.ReadWrite);
            Link = new NetworkLink(this);

            // let's find out to what interface the character device file belongs
            var deviceType = new UnixFileInfo(device).DeviceType;
            var majorNumber = deviceType >> 24;
            var minorNumber = deviceType & 0xFFFFFF;
            if(majorNumber != ExpectedMajorNumber)
            {
                throw new ConstructionException(string.Format("Unexpected major device number for OS X's tap: {0}.", majorNumber));
            }
            networkInterface = NetworkInterface.GetAllNetworkInterfaces().Single(x => x.Name == "tap" + minorNumber);
            MAC = (MACAddress)networkInterface.GetPhysicalAddress();
        }

        public void ReceiveFrame(EthernetFrame frame)
        {
            var bytes = frame.Bytes;
            deviceFile.Write(bytes, 0, bytes.Length);
            deviceFile.Flush();
            this.NoisyLog("Frame of length {0} sent to host.", frame.Length);
        }

        public NetworkLink Link { get; private set; }

        public MACAddress MAC { get; set; }

        public void Start()
        {
            Resume();
        }

        public void Pause()
        {
            cts.Cancel();
            readerTask.Wait();
        }

        public void Resume()
        {
            cts = new CancellationTokenSource();
            readerTask = Task.Run(ReadPacketAsync);
        }

        public void Dispose()
        {
            deviceFile.Close();
        }

        private async Task ReadPacketAsync()
        {
            var buffer = new byte[Mtu];
            while(!cts.IsCancellationRequested)
            {
                try
                {
                    if(await deviceFile.ReadAsync(buffer, 0, buffer.Length, cts.Token) > 0)
                    {
                        var frame = new EthernetFrame(buffer, true);
                        Link.TransmitFrameFromInterface(frame);
                        this.NoisyLog("Frame of length {0} received from host.", frame.Length);
                    }
                }
                catch(IOException)
                {
                    if(networkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        this.NoisyLog("I/O exception while interface is not up, waiting {0}s.", Misc.NormalizeDecimal(GracePeriod.TotalSeconds));
                        // probably the interface is not opened yet
                        await Task.Delay(GracePeriod);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private Task readerTask;
        private CancellationTokenSource cts;
        private readonly FileStream deviceFile;
        private readonly NetworkInterface networkInterface;

        private static readonly TimeSpan GracePeriod = TimeSpan.FromSeconds(1);
        private const int Mtu = 1500;
        private const int ExpectedMajorNumber = 20;
    }
}

