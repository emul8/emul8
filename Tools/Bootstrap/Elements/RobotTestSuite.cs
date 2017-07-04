//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Emul8.Bootstrap.Elements
{
    [DebuggerDisplay ("Path = {Path}, IsExcluded = {IsExcluded}")]
    public class RobotTestSuite : IInterestingElement
    {
        public static bool TryCreate(string path, out IInterestingElement result)
        {
            result = new RobotTestSuite(path);
            return !((RobotTestSuite)result).IsExcluded;
        }

        public RobotTestSuite(string path)
        {
            Path = System.IO.Path.GetFullPath(path);
            using(var reader = File.OpenText(path))
            {
                var firstLine = reader.ReadLine();
                IsExcluded = Regex.IsMatch(firstLine, @"#\s*[Ii]gnore\s+[Tt]est");
            }
        }

        public string Path { get; private set; }
        public bool IsExcluded { get; private set; }
    }
}

