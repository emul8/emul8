//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
namespace Emul8.Bootstrap.Elements
{
    public class RobotTestSuite : IInterestingElement
    {
        public static bool TryCreate(string path, out IInterestingElement result)
        {
            result = new RobotTestSuite(path);
            return true;
        }

        public RobotTestSuite(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
        }

        public string Path { get; private set; }
    }
}

