//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;
using Emul8.Peripherals.Bus;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Antmicro.Migrant;
using Emul8.Core;

namespace UnitTests
{
	[TestFixture]
	public class SerializableMappedSegmentTests
	{
		[Test]
		public void ShouldSerializeWhenNotTouched()
		{
			var testedPeripheral = new PeripheralWithMappedSegment();
			var copy = Serializer.DeepClone(testedPeripheral);
			var segments = copy.MappedSegments.ToArray();
			foreach(var segment in segments)
			{
				Assert.AreEqual(IntPtr.Zero, segment.Pointer);
			}
		}
		
		[Test]
		public void ShouldSerializeWhenTouched()
		{
			var testedPeripheral = new PeripheralWithMappedSegment();
			testedPeripheral.Touch();
			var copy = Serializer.DeepClone(testedPeripheral);
			var segments = copy.MappedSegments.ToArray();
			foreach(var segment in segments)
			{
				Assert.AreNotEqual(IntPtr.Zero, segment.Pointer);
			}
		}
	}
	
	public sealed class PeripheralWithMappedSegment : IBytePeripheral, IMapped
	{
		public PeripheralWithMappedSegment()
		{
			segments = new [] { new SerializableMappedSegment(4096, 0), new SerializableMappedSegment(4096, 8192) };
		}
		
		public IEnumerable<IMappedSegment> MappedSegments
		{
			get
			{
				return segments;
			}
		}
		
		public void Touch()
		{
			foreach(var s in segments)
			{
				s.Touch();
			}
		}

		public void Reset()
		{
			
		}

		public byte ReadByte(long offset)
		{
			return 0;
		}

		public void WriteByte(long offset, byte value)
		{
			
		}
		
		private readonly SerializableMappedSegment[] segments;
		
	}
}

