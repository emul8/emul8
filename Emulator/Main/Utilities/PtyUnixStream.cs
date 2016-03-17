//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Mono.Unix.Native;
using Mono.Unix;
using System.Runtime.InteropServices;
using System.IO;
using Antmicro.Migrant;
using Antmicro.Migrant.Hooks;

namespace Emul8.Utilities
{
    public class PtyUnixStream : Stream
    {
        public PtyUnixStream(string name)
        {
            this.name = name;
            Init();
        }

        public PtyUnixStream()
        {
            Init();
        }

        [PostDeserialization]
        private void Init()
        {
            if (name != null)
            {
                OpenPty(name);
                Name = name;
            }
            else
            {
                Name = OpenPty();
            }
        }

        private string name;

        [field: Transient]
        public string Name { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            disposed = true;
        }

        #region Stream

        public override int ReadTimeout { get; set; }

        public override int ReadByte()
        {
            int result;
            try {
                if (ReadTimeout > 0)
                {
                    result = IsDataAvailable(ReadTimeout) ? base.ReadByte() : -2;
                    ReadTimeout = -1;
                }
                else
                {
                    result = base.ReadByte();
                }

                return result;
            }
            catch (IOException)
            {
                return -1;
            }
        }

        public override void Flush()
        {
            Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        public override bool CanRead { get { return Stream.CanRead; } }

        public override bool CanSeek { get { return Stream.CanSeek; } }

        public override bool CanWrite { get { return Stream.CanWrite; } }

        public override long Length { get { return Stream.Length; } }

        public override long Position 
        {
            get { return Stream.Position; }
            set { Stream.Position = value; }
        }

        public override bool CanTimeout { get { return true; } }

        #endregion

        private void OpenPty(string name)
        {
            var openFlags = OpenFlags.O_NOCTTY | OpenFlags.O_RDWR;
            /*if(nonBlocking)
            {
                openFlags |= OpenFlags.O_NONBLOCK;
            }*/

            master = Syscall.open(name, openFlags);
            UnixMarshal.ThrowExceptionForLastErrorIf(master);

            IntPtr termios = Marshal.AllocHGlobal(128); // termios struct is 60-bytes, but we allocate more just to make sure
            Tcgetattr(0, termios);
            Cfmakeraw(termios);
            Tcsetattr(master, 1, termios);  // 1 == TCSAFLUSH
            Marshal.FreeHGlobal(termios);
        }

        private string OpenPty()
        {
            OpenPty("/dev/ptmx");

            var gptResult = Grantpt(master);
            UnixMarshal.ThrowExceptionForLastErrorIf(gptResult);
            var uptResult = Unlockpt(master);
            UnixMarshal.ThrowExceptionForLastErrorIf(uptResult);
            var slaveName = GetPtyName(master);

            return slaveName;
        }

        private static string GetPtyName(int fd)
        {
            return Marshal.PtrToStringAnsi(Ptsname(fd));
        }

        private bool IsDataAvailable(int timeout, out int pollResult)
        {
            var pollData = new Pollfd {
                fd = master,
                events = PollEvents.POLLIN
            };
            do
            {
                pollResult = Syscall.poll(new [] { pollData }, timeout);
            }
            while(!disposed && UnixMarshal.ShouldRetrySyscall(pollResult));
            return pollResult > 0;
        }

        private bool IsDataAvailable(int timeout, bool throwOnError = true)
        {
            int pollResult;
            IsDataAvailable(timeout, out pollResult);
            if(throwOnError && pollResult < 0)
            {
                UnixMarshal.ThrowExceptionForLastError();
            }
            return pollResult > 0;
        }

        [DllImport("libc", EntryPoint = "getpt")]
        private extern static int Getpt();
     
        [DllImport("libc", EntryPoint = "grantpt")]
        private extern static int Grantpt(int fd);

        [DllImport("libc", EntryPoint = "unlockpt")]
        private extern static int Unlockpt(int fd);

        [DllImport("libc", EntryPoint="ptsname")]
        extern static IntPtr Ptsname(int fd);

        [DllImport("libc", EntryPoint = "cfmakeraw")]
        private extern static void Cfmakeraw(IntPtr termios); // TODO: this is non-posix, but should work on BSD

        [DllImport("libc", EntryPoint = "tcgetattr")]
        private extern static void Tcgetattr(int fd, IntPtr termios);

        [DllImport("libc", EntryPoint = "tcsetattr")]
        private extern static void Tcsetattr(int fd, int attr, IntPtr termios);

        private UnixStream Stream 
        {
            get 
            {
                if(stream == null) 
                {
                    stream = new UnixStream(master, true);
                }
                return stream;
            }
        }

        [Transient]
        private UnixStream stream;

        [Transient]
        private int master;

        private bool disposed;
    }
}

