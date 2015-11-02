//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System;
using System.Linq;
using Emul8.Peripherals;

namespace Emul8.Config.Devices
{
	public class DeviceInfo
	{
		//<irqcontroller, <source, dest>>
		private Dictionary<string, dynamic> irqs = new Dictionary<string, dynamic>();

        public Dictionary<string, dynamic> Irqs{ get { return irqs; } }

        private Dictionary<string, dynamic> irqsFrom = new Dictionary<string, dynamic>();

        public Dictionary<string, dynamic> IrqsFrom{ get { return irqsFrom; } }

		public Dictionary<string, List<IDictionary<string, object>>> Connections{ get; private set; }
		
		public IEnumerable<UInt32> Address{ get; set; }
		
		public Type Type { get; set; }

		public string Name{ get; set; }

		public bool HasConnections{ get { return Connections.Any(); } }

		public IPeripheral Peripheral{ get; set; }

		public bool IsRegistered{ get; set; }

		public DeviceInfo()
		{
			Connections = new Dictionary<string, List<IDictionary<string, object>>>();
		}

        public void AddConnection(string container)
        {
            AddConnection(container, new Dictionary<string, object>());
        }

		public void AddConnection(string container, IDictionary<string,object> connectionDef)
		{
			if(!Connections.ContainsKey(container))
			{
				Connections.Add(container, new List<IDictionary<string,object>>());
			}
			//	connectionDef.Add("name", Name);
			Connections[container].Add(connectionDef);
		}

		public void AddConnection(string container, IList<dynamic> list)
		{
			foreach(var item in list)
			{
				AddConnection(container, item);
			}
		}

		public void AddIrq(string section, dynamic value)
		{
			irqs[section] = value;
		}

        public void AddIrqFrom(string section, dynamic value)
        {
            irqsFrom[section] = value;
        }
	}
}

