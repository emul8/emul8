//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Core.Structure;
using Emul8.Utilities;
using Antmicro.Migrant;
using Antmicro.Migrant.Hooks;
using Emul8.Peripherals;

namespace Emul8.Core
{
    public sealed class PeripheralTreeEntry
    {
        public PeripheralTreeEntry(IPeripheral peripheral, IPeripheral parent, Type type, IRegistrationPoint registrationPoint, string name, int level)
        {
            this.type = type;
            Name = name;
            RegistrationPoint = registrationPoint;
            Peripheral = peripheral;
            Parent = parent;
            Level = level;
        }

        public Type Type
        {
            get
            {
                return type;
            }
        }

        public override string ToString()
        {
            return string.Format("[PeripheralTreeEntry: Type={0}, Peripheral={1}, Parent={2}, Name={3}, Level={4}, RegistrationPoint={5}]",
                Type, Peripheral.GetType(), Parent == null ? "(none)" : Parent.GetType().ToString(), Name, Level, RegistrationPoint);
        }

        public void Reparent(PeripheralTreeEntry entry)
        {
            Parent = entry.Parent;
            Level = entry.Level + 1;
        }

        public IPeripheral Peripheral { get; private set; }
        public IPeripheral Parent { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; }
        public IRegistrationPoint RegistrationPoint { get; private set; }

        [PreSerialization]
        private void SaveType()
        {
            typeName = type.FullName;
        }

        [PostDeserialization]
        private void RecoverType()
        {
            type = TypeManager.Instance.GetTypeByName(typeName);
            typeName = null;
        }

        [Transient]
        private Type type;
        private string typeName;
    }
}

