//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Utilities.GDB.Commands
{
    internal class WriteDataToMemoryCommand : Command
    {
        public WriteDataToMemoryCommand(CommandsManager manager) : base(manager)
        {
        }

        [Execute("M")]
        public PacketData WriteHexData(
           [Argument(Separator = ',', Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]uint address,
           [Argument(Separator = ':', Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]uint length,
           [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.HexBytesString)]byte[] data)
        {
            manager.Machine.SystemBus.WriteBytes(data, address);
            return PacketData.Success;
        }

        [Execute("X")]
        public PacketData WriteBinaryData(
           [Argument(Separator = ',', Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]uint address,
           [Argument(Separator = ':', Encoding = ArgumentAttribute.ArgumentEncoding.HexNumber)]uint length,
           [Argument(Encoding = ArgumentAttribute.ArgumentEncoding.BinaryBytes)]byte[] data)
        {
            manager.Machine.SystemBus.WriteBytes(data, address);
            return PacketData.Success;
        }
    }
}

