//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Collections.Generic;
using System.Xml.XPath;
using System.Linq;

namespace Emul8.Bootstrap.Elements.Projects
{
    public class PluginProject : Project
    {
        public PluginProject(string name, string path) : base(name, path)
        {
        }
        
        protected override bool TryLoad(System.Xml.Linq.XDocument doc)
        {
            var projectInfoNode = doc.XPathSelectElement(ProjectInfoXPath, NamespaceManager);
            PluginModes = projectInfoNode.Elements().Select(x => x.Value).ToArray();
            return base.TryLoad(doc);
        }
        
        public IEnumerable<string> PluginModes { get; private set; }
    }
}

