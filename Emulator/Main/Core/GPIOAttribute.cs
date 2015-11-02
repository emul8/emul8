//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System;

namespace Emul8.Core
{
    [AttributeUsage(AttributeTargets.Class)]
	public class GPIOAttribute : Attribute
	{
		/// <summary>
		/// Specifies number of GPIO inputs. If it is 0 (default), the number of inputs is unbound.
		/// </summary>
		public int NumberOfInputs
		{
			get;
			set;
		}
		
		/// <summary>
		/// Specifies number of GPIO outputs. If it is 0 (default), the number of outputs is unbound.
		/// </summary>
		public int NumberOfOutputs
		{
			get;
			set;
		}
		
	}
}

