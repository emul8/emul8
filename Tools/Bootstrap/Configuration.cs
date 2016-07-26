//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Elements.Projects;

namespace Emul8.Bootstrap
{
    public class Configuration
    {
        public Configuration(Solution solution, IEnumerable<RobotTestSuite> robotTests)
        {
            Solution = solution;
            RobotTests = robotTests;
        }

        public void Save(string directory)
        {
            Directory.CreateDirectory(directory);

            // generate entry project
            var entryProject = Solution.Projects.OfType<EntryProject>().SingleOrDefault();
            if(entryProject != null)
            {
                entryProject.Save(directory);
            }

            // generate solution file
            File.WriteAllText(Path.Combine(directory, solutionName), Solution.ToString());

            // generate tests file
            var testsFilePath = Path.Combine(directory, testsFileName);
            File.WriteAllLines(testsFilePath, Solution.Projects.OfType<TestsProject>().Select(x => x.Path)
                .Union(RobotTests == null ? Enumerable.Empty<string>() : RobotTests.Select(x => x.Path)));
        }

        public Solution Solution { get; private set; }
        public IEnumerable<RobotTestSuite> RobotTests { get; private set; }

        private const string solutionName = "Emul8.sln";
        private const string testsFileName = "tests.txt";
    }
}

