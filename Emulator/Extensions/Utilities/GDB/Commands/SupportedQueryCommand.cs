//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Utilities.GDB.Commands
{
    [Mnemonic("q")]
    internal class SupportedQueryCommand : Command
    {
        protected override PacketData HandleInner(Packet packet)
        {
            if(packet.Data.DataAsString.StartsWith("qSupported", System.StringComparison.Ordinal))
            {
                return new PacketData(string.Format("PacketSize={0:x4}", 4096));
            }

            return PacketData.Empty;
        }
    }
}

