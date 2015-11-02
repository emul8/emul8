//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Core;
using System.Linq;
using System.Collections.Generic;
using System;
using Emul8.UserInterface;

namespace Emul8.Peripherals
{
    [Icon("box")]
    public interface IPeripheral : IEmulationElement, IAnalyzable
	{
		void Reset();
	}

    public static class IPeripheralExtensions
    {
        public static bool HasGPIO(this IPeripheral peripheral)
        {
            return peripheral is INumberedGPIOOutput || peripheral.GetType().GetProperties().Any(x => x.PropertyType == typeof(GPIO));
        }

        /// <summary>
        /// This method returns connected GPIO endpoints of a given peripheral.
        /// </summary>
        /// <returns>Collection of tuples: local GPIO name maped on endpoint to which it is connected. In case of INumberedGPIOOutput name is local number</returns>
        /// <param name="peripheral">Peripheral.</param>
        public static IEnumerable<Tuple<string, IGPIO>> GetGPIOs(this IPeripheral peripheral)
        {
            IEnumerable<Tuple<string, IGPIO>> result = null;
            var numberGPIOOuput = peripheral as INumberedGPIOOutput;
            if(numberGPIOOuput != null)
            {
                result = numberGPIOOuput.Connections.Select(x => Tuple.Create(x.Key.ToString(), x.Value));
            }

            var local = peripheral.GetType().GetProperties().Where(x => x.PropertyType == typeof(GPIO)).Select(x => Tuple.Create(x.Name, (IGPIO)((GPIO)x.GetValue(peripheral))));
            return result == null ? local : result.Union(local);
        }
    }
}

