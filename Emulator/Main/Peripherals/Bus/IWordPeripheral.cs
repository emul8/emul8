//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//


namespace Emul8.Peripherals.Bus
{
	public interface IWordPeripheral : IBusPeripheral
	{
		ushort ReadWord(long offset);
		void WriteWord(long offset, ushort value);
	}
}
