//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using Emul8.Peripherals;

namespace Emul8.Core.Structure
{
	
	/// <summary>
	/// Interface representing registered device. It is covariant because registered specialised device is
	/// registered device.
	/// </summary>
	public interface IRegistered<out TPeripheral, TRegistrationPoint>
        where TPeripheral : IPeripheral where TRegistrationPoint : IRegistrationPoint
	{
		TPeripheral Peripheral { get; }
        TRegistrationPoint RegistrationPoint { get; }
	}
}
