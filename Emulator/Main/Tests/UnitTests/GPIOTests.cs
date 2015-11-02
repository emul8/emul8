//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;
using Emul8.Core;
using NUnit.Framework;
using UnitTests.Mocks;
using Emul8.Exceptions;

namespace UnitTests
{
	[TestFixture]
	public class GPIOTests
	{
		
		[Test]
		public void ShouldPropagateConnected()
		{
			var source = new GPIO();
			var destination = new MockReceiver();
			source.Connect(destination, 2);
			var endpoint = source.Endpoint;
			Assert.AreEqual(2, endpoint.Number);
			Assert.AreEqual(destination, endpoint.Receiver);			
		}

		[Test]
		public void ShouldGiveNullOnNotConnected()
		{
			var source = new GPIO();
			var endpoint = source.Endpoint;
			Assert.AreEqual(null, endpoint);			
		}
		
		[Test]
		public void ShouldConnectBoundGPIOs()
		{
			var source = new GPIO();
			var boundIn = new MockReceiverConstrained();
			source.Connect(boundIn, 2);
		}
		
		[Test]
        [ExpectedException(typeof(ConstructionException), UserMessage = NonExistingGPIO)]
		public void ShouldThrowOnIllegalInputNo()
		{
			var source = new GPIO();
			var boundIn = new MockReceiverConstrained();
			source.Connect(boundIn, 10);
		}

		private const string NonExistingGPIO = "Connector perimtted to connect to non existing GPIO.";
	}
	

}
