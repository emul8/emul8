//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Backends.Display;
using Emul8.Core;
using Emul8.Peripherals.Bus;
using Emul8.Logging;

namespace Emul8.Peripherals.Video
{
    public class TegraDisplay : AutoRepaintingVideo, IDoubleWordPeripheral, IKnownSize
    {
        public TegraDisplay(Machine machine) : base(machine)
        {
            Reconfigure(640, 480, PixelFormat.RGB565);
            this.machine = machine;
            sync = new object();
        }

        public long Size
        {
            get
            {
                return 0x40000;
            }
        }

        public void WriteDoubleWord(long address, uint value)
        {
            if(address == 0x1c14) // DC_WIN_SIZE
            {
                int w = (int)(value & 0xFFFF);
                int h = (int)((value >> 16) & 0xFFFF);
                this.DebugLog("Setting resolution to {0}x{1}", w, h);
                Reconfigure(w, h);
            }
    	    if(address == 0x1c0c) // DC_WIN_COLOR_DEPTH
    	    {
    	    	this.Log(LogLevel.Warning, "Depth ID={0}", value);
        		lock (sync) {
        			switch (value) 
                    {
                    case 3:
                        Reconfigure(format: PixelFormat.RGB565);
    					break;
                    case 12:
                        Reconfigure(format: PixelFormat.BGRX8888);
    					break;
                    case 13:
                        Reconfigure(format: PixelFormat.RGBX8888);
    					break;
                    default:
                        this.Log(LogLevel.Warning, "Depth ID={0} is not supported (might be YUV)!", value);
                        Reconfigure(format: PixelFormat.RGB565);
    					break;
        			}
        		}
    	    }
            if(address == 0x2000) // DC_WINBUF_START_ADDR
            {
                this.DebugLog("Setting buffer addr to 0x{0:X}", value);
                lock (sync) {
                        bufferAddress = value;
                }
            }
        }

        public uint ReadDoubleWord(long offset)
        {
            return 0x00;
        }

        public override void Reset()
        {
            // TODO!
        }

        protected override void Repaint()
        {
            if(bufferAddress == 0xFFFFFFFF)
            {
                return;
            }
            lock (sync) 
            {
                machine.SystemBus.ReadBytes(bufferAddress, buffer.Length, buffer, 0);
            }
        }

        private object sync;
        private uint bufferAddress = 0xFFFFFFFF;
        private readonly Machine machine;
    }
}

