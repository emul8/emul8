//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Emul8.Bootstrap
{
    [DebuggerDisplay("Name = {Name}")]
    public class Project
    {
        static Project()
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("x", @"http://schemas.microsoft.com/developer/msbuild/2003");
            NamespaceManager = namespaceManager;
        }

        public static bool TryLoadFromFile(string path, out Project project)
        {
            if(string.IsNullOrEmpty(path))
            {
                project = null;
                return false;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(path);
            }
            catch (DirectoryNotFoundException)
            {
                project = null;
                return false;
            }

            var nameNode = doc.XPathSelectElement(@"/x:Project/x:PropertyGroup/x:AssemblyName", NamespaceManager);
            var projectInfoNode = doc.XPathSelectElement(@"/x:Project/x:PropertyGroup/x:ProjectInfo", NamespaceManager);
            XAttribute projectTypeAttribute = null;
            if(projectInfoNode != null)
            {
                projectTypeAttribute = projectInfoNode.Attribute("Type");
            }
            if(nameNode == null)
            {
                project = null;
                return false;
            }
            if(projectInfoNode != null)
            {
                var skippedNode = projectInfoNode.Attribute("Skip");
                if(skippedNode != null && skippedNode.Value == "true")
                {
                    project = null;
                    return false;
                }

                var type = projectTypeAttribute != null ? ProjectTypeHelper.Parse(projectTypeAttribute.Value) : ProjectType.Unknown;
                project = type.CreateInstance(nameNode.Value, path);
            }
            else
            {
                project = new UnknownProject(nameNode.Value, path);
            }

            return project.TryLoad(doc);
        }

        public static Project CreateEntryProject(Project mainProject, IEnumerable<Project> additionalProjects)
        {
            return new CustomProject(mainProject.StartupObject, new [] { mainProject }.Union(additionalProjects));
        }

        protected static readonly IXmlNamespaceResolver NamespaceManager;

        public IEnumerable<Project> GetAllReferences()
        {
            var result = new List<Project>();
            var toProcess = new Queue<Project>();
            toProcess.Enqueue(this);

            while(toProcess.Any())
            {
                var current = toProcess.Dequeue();
                if(result.Contains(current))
                {
                    // why do we remove it here? so that we maintain proper ordering when result is reversed
                    result.Remove(current);
                }

                result.Add(current);
                foreach(var reference in current.References)
                {
                    toProcess.Enqueue(reference);
                }
            }

            result.Reverse();
            return result;
        }

        public override bool Equals(object obj)
        {
            var objAsProject = obj as Project;
            if(objAsProject != null)
            {
                return objAsProject.GUID == GUID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return GUID.GetHashCode();
        }

        public string Target { get; protected set; }
        public string Path { get; protected set; }
        public string Name { get; private set; }
        public Guid GUID { get; protected set; }
        public bool HasForcedOutput { get; private set; }
        public IEnumerable<Project> References { get; protected set; }
        public string StartupObject { get; protected set; }

        protected Project(string name, string path)
        {
            Path = path;
            Name = name;
        }

        protected virtual bool TryLoad(XDocument doc)
        {
            var guidNode = doc.XPathSelectElement(@"/x:Project/x:PropertyGroup/x:ProjectGuid", NamespaceManager);
            var targetNode = doc.XPathSelectElements(@"/x:Project/x:PropertyGroup/x:PlatformTarget", NamespaceManager).FirstOrDefault();
            var startupObjectNode = doc.XPathSelectElement(@"/x:Project/x:PropertyGroup/x:StartupObject", NamespaceManager);
            var referenceNodes = doc.XPathSelectElements(@"/x:Project/x:ItemGroup/x:ProjectReference", NamespaceManager).ToList();
            var forcedOutputNode = doc.XPathSelectElement(@"/x:Project/x:Target[@Name='GetForcedOutput']", NamespaceManager);

            GUID = Guid.Parse(guidNode.Value);
            StartupObject = startupObjectNode != null ? startupObjectNode.Value : string.Empty;
            Target = targetNode != null ? targetNode.Value : "Any CPU";
            var dir = System.IO.Path.GetDirectoryName(Path);

            var references = new List<Project>();
            foreach(var node in referenceNodes)
            {
                var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, node.Attribute("Include").Value.Replace(@"\", "/")));
                Project project;
                if(!Project.TryLoadFromFile(path, out project))
                {
                    return false;
                }
                references.Add(project);
            }

            References = references;
            HasForcedOutput = (forcedOutputNode != null);
            return true;
        }
    }
}
