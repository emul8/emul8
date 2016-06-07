//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Emul8.Robot;
using Emul8.Utilities;

namespace Emul8.RobotFrontend
{
    public class RobotFrontend
    {
        public static void Main(string[] args)
        {
            int port;
            if(args.Length != 1 || !int.TryParse(args[0], out port))
            {
                Console.Error.WriteLine("Provide valid port number as an argument.");
                return;
            }

            var keywordManager = new KeywordManager();
            TypeManager.Instance.AutoLoadedType += keywordManager.Register;

            var processor = new XmlRpcServer(keywordManager);
            server = new HttpServer(processor);
            server.Run(port);
            server.Dispose();
        }

        public static void Shutdown()
        {
            server.Shutdown();
        }

        private static HttpServer server;
    }
}
