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

namespace Emul8.Peripherals.Bus
{
    public interface IBusRegistered<out T> : IRegistered<T, BusRangeRegistration> where T : IBusPeripheral
    {
    }

    public static class IRegisteredExtensions
    {
        public static IBusRegistered<TTo> Convert<TFrom, TTo>(this IBusRegistered<TFrom> conversionSource) where TTo : TFrom where TFrom : IBusPeripheral
        {
            return new BusRegistered<TTo>((TTo)conversionSource.Peripheral, new BusRangeRegistration(conversionSource.RegistrationPoint.Range,
                                          conversionSource.RegistrationPoint.Offset));
        }

        public static IEnumerable<IBusRegistered<TTo>> Convert<TFrom, TTo>(this IEnumerable<IBusRegistered<TFrom>> sourceCollection) where TTo : TFrom where TFrom : IBusPeripheral
        {
            return sourceCollection.Select(x => x.Convert<TFrom, TTo>());
        }
    }
}

