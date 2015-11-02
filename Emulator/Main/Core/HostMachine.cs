//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core.Structure;
using System.Collections.Generic;
using System.Linq;
using System;
using Antmicro.Migrant;
using Emul8.Peripherals;
using Emul8.Exceptions;
using Emul8.UserInterface;

namespace Emul8.Core
{
    [Icon("comp")]
    [ControllerMask(typeof(IExternal))]
    public class HostMachine : IExternal, IHasChildren<IHostMachineElement>, IDisposable
    {
        public HostMachine()
        {
            hostEmulationElements = new Dictionary<string, IHostMachineElement>();
        }

        public void AddHostMachineElement(IHostMachineElement element, string name)
        {
            if(hostEmulationElements.ContainsKey(name))
            {
                throw new RecoverableException("Element with the same name already exists");
            }

            hostEmulationElements.Add(name, element);

            var elementAsIHasOwnLife = element as IHasOwnLife;
            if(elementAsIHasOwnLife != null)
            {
                EmulationManager.Instance.CurrentEmulation.ExternalsManager.RegisterIHasOwnLife(elementAsIHasOwnLife);
            }

            var cc = ContentChanged;
            if(cc != null)
            {
                cc();
            }
        }

        public void RemoveHostMachineElement(string name)
        {
            RemoveHostMachineElement(hostEmulationElements[name]);
        }

        public void RemoveHostMachineElement(IHostMachineElement element)
        {
            var elementAsIHasOwnLife = element as IHasOwnLife;
            if(elementAsIHasOwnLife != null)
            {
                EmulationManager.Instance.CurrentEmulation.ExternalsManager.UnregisterIHasOwnLife(elementAsIHasOwnLife);
            }

            var key = hostEmulationElements.SingleOrDefault(x => x.Value == element).Key;
            hostEmulationElements.Remove(key);

            var cc = ContentChanged;
            if(cc != null)
            {
                cc();
            }

        }

        public IEnumerable<string> GetNames()
        {
            return hostEmulationElements.Keys;
        }

        public IHostMachineElement TryGetByName(string name, out bool success)
        {
            IHostMachineElement value;
            success = hostEmulationElements.TryGetValue(name, out value);
            return value;
        }

        public bool TryGetName(IHostMachineElement element, out string name)
        {
            var pair = hostEmulationElements.SingleOrDefault(x => x.Value == element);
            if(pair.Value == element)
            {
                name = pair.Key;
                return true;
            }

            name = null;
            return false;
        }

        #region IDisposable implementation

        public void Dispose()
        {
            foreach(var element in hostEmulationElements)
            {
                var disposable = element.Value as IDisposable;
                if(disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        #endregion

        [field: Transient]
        public event Action ContentChanged;

        private readonly Dictionary<string, IHostMachineElement> hostEmulationElements;

        public const string HostMachineName = "host";
    }
}

