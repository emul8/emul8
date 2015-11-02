//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.Migrant;
using Emul8.Core;
using Emul8.Peripherals.CPU;
using Emul8.Peripherals.Bus;

namespace UnitTests.Mocks
{
    public class EmptyCPU : ICPU
    {
		
        public EmptyCPU(Machine machine)
        {
            this.machine = machine;
        }

        public virtual void Start()
        {
			
        }

        public virtual void Pause()
        {
        }

        public virtual void Resume()
        {
			
        }

        public virtual void Reset()
        {
			
        }

        public virtual void MapMemory(IMappedSegment segment)
        {
        }

        public virtual void UnmapMemory(Range range)
        {
        }

        public virtual void UpdateContext()
        {
        }

        public virtual uint PC
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        public SystemBus Bus
        {
            get
            {
                return machine.SystemBus;
            }
        }

        public virtual string Model
        {
            get
            {
                return "empty";
            }
        }

        public virtual void Load(PrimitiveReader reader)
        {
			
        }

        public bool IsHalted
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool OnPossessedThread { get { return true; } }

        public virtual void Save(PrimitiveWriter writer)
        {
			
        }

        protected readonly Machine machine;
    }
}

