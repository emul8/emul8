﻿//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Terminal;
using Emul8.Utilities;

namespace Emul8.CLI
{
    public interface IConsoleBackendAnalyzerProvider : IAutoLoadType
    {
        bool TryOpen(string consoleName, out IIOSource io);
        void Close();
        event Action OnClose;
    }

    public class ConsoleBackendAnalyzerProviderAttribute : Attribute
    {
        public ConsoleBackendAnalyzerProviderAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

    }
}
