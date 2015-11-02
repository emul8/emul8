//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//


namespace Emul8.Peripherals.Bus
{
	public interface IDoubleWordPeripheral : IBusPeripheral
	{
		uint ReadDoubleWord(long offset);
		void WriteDoubleWord(long offset, uint value);
	}
}
