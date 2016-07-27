//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Xml.XPath;
using System;

namespace Emul8.Bootstrap.Elements.Projects
{
    public class UiProject : Project
    {
        public UiProject(string name, string path) : base(name, path)
        {
        }
        
        protected override bool TryLoad(System.Xml.Linq.XDocument doc)
        {
            var projectInfoNode = doc.XPathSelectElement(@"/x:Project/x:PropertyGroup/x:ProjectInfo", NamespaceManager);
            var uiType = projectInfoNode.Value;
            if(string.IsNullOrWhiteSpace(uiType))
            {
                throw new ArgumentException("UiType must be set.");
            }
            
            UiType = uiType;
            
            return base.TryLoad(doc);
        }
        
        public string UiType { get; private set; }
    }
}

