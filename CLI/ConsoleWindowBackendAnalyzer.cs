//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Peripherals;
using Emul8.Peripherals.UART;
using Emul8.Utilities;
using System.Linq;
using AntShell.Terminal;
using Emul8.Core;
using Emul8.Exceptions;
using Emul8.Logging;

namespace Emul8.CLI
{
    public class ConsoleWindowBackendAnalyzer : IAnalyzableBackendAnalyzer<UARTBackend>
    {
        public ConsoleWindowBackendAnalyzer()
        {
            IO = new IOProvider();
        }

        public void AttachTo(UARTBackend backend)
        {
            Backend = backend;
            if(EmulationManager.Instance.CurrentEmulation.TryGetEmulationElementName(backend.UART, out var uartName))
            {
                Name = uartName;
            }
        }

        public void Show()
        {
            var availableProviders = TypeManager.Instance.AutoLoadedTypes.Where(x => !x.IsAbstract && typeof(IConsoleBackendAnalyzerProvider).IsAssignableFrom(x)).ToDictionary(x => GetProviderName(x), x => x);
            var preferredProvider = ConfigurationManager.Instance.Get("general", "terminal", "XTerm");

            foreach(var providerName in availableProviders.Keys.OrderByDescending(x => x == preferredProvider))
            {
                var providerType = availableProviders[providerName];
                if(providerType.GetConstructor(new Type[0]) == null)
                {
                    Logger.Log(LogLevel.Warning, "There is no default public constructor for {0} console backend analyzer provider. Skipping it.", providerName);
                    continue;
                }
                provider = (IConsoleBackendAnalyzerProvider)Activator.CreateInstance(availableProviders[providerName]);
                provider.OnClose += OnClose;
                if(!provider.TryOpen(Name, out IIOSource ioSource))
                {
                    Logger.Log(LogLevel.Warning, "Could not open {0} console backend analyzer provider. Trying the next one.", providerName);
                    continue;
                }
                IO.Backend = ioSource;
                if(Backend != null)
                {
                    ((UARTBackend)Backend).BindAnalyzer(IO);
                }
                return;
            }

            throw new InvalidOperationException($"Could not start any console backend analyzer. Tried: {(string.Join(", ", availableProviders.Keys))}.");
        }

        public void Hide()
        {
            var p = provider;
            if(p == null)
            {
                return;
            }

            if(Backend != null)
            {
                ((UARTBackend)Backend).UnbindAnalyzer(IO);
                Backend = null;
            }
            p.Close();
            provider = null;
        }

        public string Name { get; private set; }

        public IAnalyzableBackend Backend { get; private set; }

        public IOProvider IO { get; private set; }

        public event Action Quitted;

        private string GetProviderName(Type type)
        {
            var attribute = type.GetCustomAttributes(false).OfType<ConsoleBackendAnalyzerProviderAttribute>().SingleOrDefault();
            if(attribute != null)
            {
                return attribute.Name;
            }

            return type.Name.EndsWith("Provider", StringComparison.Ordinal) ? type.Name.Substring(0, type.Name.Length - 8) : type.Name;
        }

        private void OnClose()
        {
            Quitted?.Invoke();
        }

        private IConsoleBackendAnalyzerProvider provider;
    }
}
