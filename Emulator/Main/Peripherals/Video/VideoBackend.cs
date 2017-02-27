//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Peripherals;
using Emul8.Peripherals.Video;
using Emul8.Backends.Display;
using ELFSharp.ELF;

namespace Emul8.Backends.Video
{
    public class VideoBackend : IAnalyzableBackend<IVideo>
    {
        public void Attach(IVideo peripheral)
        {
            Video = peripheral;
            Video.FrameRendered += HandleFrameRendered;
            Video.ConfigurationChanged += HandleConfigurationChanged;
        }
       
        private void HandleFrameRendered(byte[] frame)
        {
            if(frame != null)
            {
                Frame = frame;
            }
        }

        private void HandleConfigurationChanged(int width, int height, PixelFormat format, Endianess endianess)
        {
            Width = width;
            Height = height;
            Format = format;
            Endianess = endianess;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelFormat Format { get; private set; }
        public Endianess Endianess { get; private set; }

        public byte[] Frame { get; private set; }

        public IVideo Video { get; private set; }
        public IAnalyzable AnalyzableElement { get { return Video; } }
    }
}

