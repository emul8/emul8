//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Logging;

namespace Emul8.Utilities.GDB
{
    internal abstract class Command
    {
        public PacketData Handle(Packet packet)
        {
            try
            {
                return HandleInner(packet);
            }
            catch(Exception e)
            {
                Logger.Log(LogLevel.Warning, "Error while handling GDB packet: {0}", e.Message);
                return PacketData.Empty;
            }
        }

        public string Mnemonic { get; private set; }

        protected Command(string mnemonic)
        {
            Mnemonic = mnemonic;
        }

        protected string[] GetCommandArguments(PacketData data, char[] separators = null, int count = 1)
        {
            var arguments = data.DataAsString.Substring(Mnemonic.Length);
            if(separators == null)
            {
                return new [] { arguments };
            }
            return arguments.Split(separators, count);
        }

        protected abstract PacketData HandleInner(Packet packet);
    }
}

