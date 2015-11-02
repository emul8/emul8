//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core;
using Emul8.Peripherals;
using Emul8.Peripherals.CPU;
using Emul8.Utilities;
using NUnit.Framework;
using Moq;
using UnitTests.Mocks;
using System.Threading;

namespace UnitTests
{
	[TestFixture]
	public class MultiCPUTests
	{
		[Test]
		public void ShouldEnumerateCPUs()
		{
			const int numberOfCpus = 10;
			var cpus = new ICPU[numberOfCpus];
			using(var machine = new Machine())
			{
				var sysbus = machine.SystemBus;
				for(var i = 0; i < cpus.Length; i++)
				{
					cpus[i] = new Mock<ICPU>().Object;
					sysbus.Register(cpus[i], new CPURegistrationPoint());
				}
				for(var i = 0; i < cpus.Length; i++)
				{
					Assert.AreEqual(i, sysbus.GetCPUId(cpus[i]));
				}
			}
		}
		
		[Test]
		public void ShouldGuardPeripheralReads([Range(1, 4)] int cpuCount)
		{
			using(var machine = new Machine())
			{
				var sysbus = machine.SystemBus;
				cpuCount.Times(() => sysbus.Register(new ActivelyAskingCPU(machine, 0), new CPURegistrationPoint()));
				var peripheral = new ActivelyAskedPeripheral(); 
				sysbus.Register(peripheral, 0.To(1000));
				machine.Start();
				Thread.Sleep(1000);
				machine.Pause();
				Assert.IsFalse(peripheral.Failed, "Peripheral was concurrently accessed from multiple CPUs.");
			}
		}
	}
}

