//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Peripherals.Bus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowedTranslationsAttribute : Attribute
    {
        public AllowedTranslationsAttribute(AllowedTranslation allowedTranslations)
        {
            this.allowedTranslations = allowedTranslations;
        }

        public AllowedTranslation AllowedTranslations
        {
            get
            {
                return allowedTranslations;
            }
        }

        private readonly AllowedTranslation allowedTranslations;
    }
}

