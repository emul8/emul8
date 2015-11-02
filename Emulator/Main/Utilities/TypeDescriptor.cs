//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Mono.Cecil;

namespace Emul8.Utilities
{
    public class TypeDescriptor
    {
        public TypeDescriptor(Type t)
        {
            underlyingType = t;
        }

        public TypeDescriptor(TypeDefinition t)
        {
            underlyingType = t;
        }

        public string Name
        {
            get
            {
                var type = underlyingType as Type;
                if (type != null)
                {
                    return type.Name;
                }

                var typeDefinition = underlyingType as TypeDefinition;
                if (typeDefinition != null)
                {
                    return typeDefinition.Name;
                }

                throw new ArgumentException("Unsupported underlying type: " + underlyingType.GetType().FullName);
            }
        }

        public string Namespace
        {
            get
            {
                var type = underlyingType as Type;
                if (type != null)
                {
                    return type.Namespace;
                }

                var typeDefinition = underlyingType as TypeDefinition;
                if (typeDefinition != null)
                {
                    return typeDefinition.Namespace;
                }

                throw new ArgumentException("Unsupported underlying type: " + underlyingType.GetType().FullName);
            }

        }

        public Type ResolveType()
        {
            var type = underlyingType as Type;
            if (type != null)
            {
                return type;
            }

            var typeDefinition = underlyingType as TypeDefinition;
            if (typeDefinition != null)
            {
                return TypeResolver.ResolveType(typeDefinition.FullName) ??
                    TypeManager.Instance.GetTypeByName(typeDefinition.FullName);
            }

            throw new ArgumentException("Unsupported underlying type: " + underlyingType.GetType().FullName);
        }

        private readonly object underlyingType;
    }
}

