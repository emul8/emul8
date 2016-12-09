//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Mono.Cecil;
using System;

namespace Emul8.Utilities
{
    internal static class MemberReferenceExtensions
    {
        public static String GetFullNameOfMember(this MemberReference definition)
        {
            return definition.FullName.Replace('/', '+');            
        }        
    }
}
