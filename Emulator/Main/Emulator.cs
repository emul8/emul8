//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Emul8.Exceptions;
using Emul8.UserInterface;

namespace Emul8
{
    public static class Emulator
    {
        public static IUserInterfaceProvider UserInterfaceProvider
        {
            get
            {
                if(userInterfaceProvider == null)
                {
                    throw new RecoverableException("User interface provider not set");
                }
                return userInterfaceProvider;
            }
            set
            {
                userInterfaceProvider = value;
            }
        }

        private static IUserInterfaceProvider userInterfaceProvider;
    }
}

