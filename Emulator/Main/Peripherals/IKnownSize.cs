//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Peripherals.Bus;


namespace Emul8.Peripherals
{
	public interface IKnownSize : IBusPeripheral
	{
		long Size { get; }
	}
}
