//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;

namespace Emul8.Bootstrap.Logging
{
    public class Logger
    {
        static Logger()
        {
            Instance = new Logger();
        }
        
        public static Logger Instance { get; private set; }

        public bool Silent { get; set; }

        public void Info(string message, params object[] args)
        {
            if(!Silent)
            {
                Console.WriteLine(message, args);
            }
        }

        public void Warning(string message, params object[] args)
        {
            Info(message, args);
        }

        private Logger()
        {
        }
    }
}

