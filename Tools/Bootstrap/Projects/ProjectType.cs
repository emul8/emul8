//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using Antmicro.OptionsParser;

namespace Emul8.Bootstrap
{
    public enum ProjectType
    {
        [Hide]
        Unknown,
        UI,
        CpuCore,
        Extension,
        Plugin,
        Tests
    }

    public static class ProjectTypeHelper
    {
        public static ProjectType Parse(string value)
        {
            ProjectType result;
            if(value != null && Enum.TryParse(value, out result))
            {
                return result;
            }
            return ProjectType.Unknown;
        }
        
        public static Type GetType(ProjectType projectType)
        {
            switch(projectType)
            {
            case ProjectType.CpuCore:
                return typeof(CpuCoreProject);
            case ProjectType.Extension:
                return typeof(ExtensionProject);
            case ProjectType.Plugin:
                return typeof(PluginProject);
            case ProjectType.Tests:
                return typeof(TestsProject);
            case ProjectType.UI:
                return typeof(UiProject);
            default:
                return typeof(UnknownProject);
            }
        }
        
        public static Project CreateInstance(this ProjectType type, string name, string path)
        {
            switch(type)
            {
            case ProjectType.UI:
                return new UiProject(name, path);
            case ProjectType.CpuCore:
                return new CpuCoreProject(name, path);
            case ProjectType.Extension:
                return new ExtensionProject(name, path);
            case ProjectType.Plugin:
                return new PluginProject(name, path);
            case ProjectType.Tests:
                return new TestsProject(name, path);
            default:
                return new UnknownProject(name, path);
            }
        }
    }
}

