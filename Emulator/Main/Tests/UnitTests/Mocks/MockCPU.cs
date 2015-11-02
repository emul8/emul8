//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.Migrant;
using Emul8.Core;

namespace UnitTests.Mocks
{
	public class MockCPU : EmptyCPU
	{		
		
		public MockCPU(Machine machine):base(machine)
		{
			
		}
		
		public override string Model
		{
			get
			{
				return "mock";
			}
		}
		
		public string Placeholder { get; set; }
		
		public override void Load(PrimitiveReader reader)
		{
			var present = reader.ReadBoolean();
			if(present)
			{
				Placeholder = reader.ReadString();
			}
		}
		
		public override void Save(PrimitiveWriter writer)
		{
			var present = Placeholder != null;
			writer.Write(present);
			if(present)
			{
				writer.Write(Placeholder);
			}
		}
	}
}

