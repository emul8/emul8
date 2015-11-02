//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace Emul8.Utilities
{
	public class MonitorInfo
	{
		public IEnumerable<MethodInfo> Methods{get;set;}
		
		public IEnumerable<PropertyInfo> Properties{get;set;}
		
        public IEnumerable<PropertyInfo> Indexers{ get; set; }

		public IEnumerable<FieldInfo> Fields{get;set;}


	    public IEnumerable<String> AllNames
	    {
	        get
	        {
	            var names = new List<String>();
	            if (Methods != null)
	            {
	                names.AddRange(Methods.Select(x => x.Name));
	            }
	            if (Properties != null)
	            {
	                names.AddRange(Properties.Select(x => x.Name));
	            }
	            if (Fields != null)
	            {
	                names.AddRange(Fields.Select(x => x.Name));
	            }
                if(Indexers != null)
                {
                    names.AddRange(Indexers.Select(x => x.Name));
                }
	            return names;
	        }
	    }
	}
}

