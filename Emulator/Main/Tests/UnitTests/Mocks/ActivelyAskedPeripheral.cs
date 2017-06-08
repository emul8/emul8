//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Threading;
using Emul8.Core;

namespace UnitTests.Mocks
{
	public class ActivelyAskedPeripheral : EmptyPeripheral
	{
		public ActivelyAskedPeripheral()
		{
			random = EmulationManager.Instance.CurrentEmulation.RndGenerator;
		}
		
		public bool Failed
		{
			get
			{
				return failed;
			}
		}
		
		public override uint ReadDoubleWord(long offset)
		{
			var value = Interlocked.Read(ref counter);
			var toWait = random.Next(spinWaitIterations);
			Thread.SpinWait(toWait);
			var exchanged = Interlocked.Exchange(ref counter, ++value);
			if(exchanged != value - 1)
			{
				failed = true;
			}
			return (uint)toWait;
		}
		
		private long counter;
		private bool failed;
		private readonly PseudorandomNumberGenerator random;
		private const int spinWaitIterations = 10000;
	}
}

