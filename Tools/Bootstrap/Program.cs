//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Antmicro.OptionsParser;
using Emul8.Bootstrap.Elements;
using Emul8.Bootstrap.Paging.Steps;
using Emul8.Bootstrap.Elements.Projects;
using Emul8.Bootstrap.Logging;

namespace Emul8.Bootstrap
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            var parser = new OptionsParser();
            if(!parser.Parse(options, args))
            {
                return 1;
            }

            Logger.Instance.Silent = !options.Verbose;
            if(options.Interactive)
            {
                return HandleInteractive(options.Directories.ToList(), options.BinariesDirectory, options.OutputDirectory, options.GenerateEntryProject);
            }

            switch(options.Action)
            {
            case Operation.GenerateAll:
                var configuration = GenerateAllProjects(options.BinariesDirectory, options.GenerateEntryProject, options.Directories);
                configuration.Save(options.OutputDirectory);
                break;
            case Operation.Clean:
                Cleaner.Clean(options.OutputDirectory);
                break;
            case Operation.GenerateSolution:
                HandleGenerateSolution(options.MainProject, options.BinariesDirectory, options.AdditionalProjects, options.RobotTests, options.OutputDirectory, options.GenerateEntryProject);
                break;
            case Operation.Scan:
                HandleScan(options.Type, options.Directories);
                break;
            }

            return 0;
        }

        private static Configuration HandleCustomSolution(string outputPath, bool generateEntryProject, IEnumerable<string> directories)
        {
            var stepManager = new StepManager();
            var pathHelper = new PathHelper(directories.Select(Path.GetFullPath));

            var uiSelectionStep = new UiStep(pathHelper);
            var robotTestsStep = new RobotTestsStep("Choose robot tests:", pathHelper);
            stepManager
                .AddStep(uiSelectionStep)
                .AddStep(new ProjectsListStep<CpuCoreProject>("Choose supported architectures:", pathHelper))
                .AddStep(new ProjectsListStep<ExtensionProject>("Choose extensions libraries:", pathHelper))
                .AddStep(new PluginStep("Choose plugins:", pathHelper))
                .AddStep(new ProjectsListStep<TestsProject>("Choose tests:", pathHelper))
                .AddStep(robotTestsStep)
                .AddStep(new ProjectsListStep<UnknownProject>("Choose other projects:", pathHelper))
                .Run();
            
            if(stepManager.IsCancelled)
            {
                return null;
            }

            return new Configuration(
                SolutionGenerator.Generate(uiSelectionStep.UIProject, generateEntryProject, outputPath,
                    stepManager.GetSteps<ProjectsListStep>().SelectMany(x => x.AdditionalProjects).Union(uiSelectionStep.UIProject.GetAllReferences())),
                robotTestsStep.SelectedTests);
        }

        private static void HandleGenerateSolution(string mainProjectPath, string binariesPath, IEnumerable<string> additionalProjectsPaths, IEnumerable<string> robotTests, string output, bool generateEntryProject)
        {
            Project mainProject;
            if(!Project.TryLoadFromFile(mainProjectPath, out mainProject))
            {
                Console.Error.WriteLine("Could not load main project");
                return;
            }

            var additionalProjects = new List<Project>();
            foreach(var additionalProjectPath in additionalProjectsPaths ?? Enumerable.Empty<string>())
            {
                Project additionalProject;
                if(!Project.TryLoadFromFile(additionalProjectPath, out additionalProject))
                {
                    Console.Error.WriteLine("Could not load additional project: {0}", additionalProject);
                    return;
                }
                additionalProjects.Add(additionalProject);
            }

            var solution = SolutionGenerator.Generate(mainProject, generateEntryProject, binariesPath, additionalProjects);
            if(output == null)
            {
                Console.WriteLine(solution);
            }
            else
            {
                var confiugration = new Configuration(solution, robotTests.Select(x => new RobotTestSuite(x)));
                confiugration.Save(output);
            }
        }

        private static bool TryFind(string command)
        {
            var verifyProc = new Process();
            verifyProc.StartInfo.UseShellExecute = false;
            verifyProc.StartInfo.RedirectStandardError = true;
            verifyProc.StartInfo.RedirectStandardInput = true;
            verifyProc.StartInfo.RedirectStandardOutput = true;
            verifyProc.EnableRaisingEvents = false;
            verifyProc.StartInfo.FileName = "which";
            verifyProc.StartInfo.Arguments = command;

            verifyProc.Start();

            verifyProc.WaitForExit();
            return verifyProc.ExitCode == 0;
        }

        private static int HandleInteractive(List<string> directories, string binariesDirectory, string outputDirectory, bool generateEntryProject)
        {
            // check if "dialog" application is available
            if(!TryFind("dialog"))
            {
                Console.Error.WriteLine("The 'dialog' application is necessary to run in interactive mode");
                return ErrorResultCode;
            }

            if(directories == null || directories.All(x => string.IsNullOrEmpty(x)))
            {
                var directoryDialog = new InputboxDialog(Title, "Provide directories to scan (colon-separated):", ".");
                if(directoryDialog.Show() != DialogResult.Ok)
                {
                    return CancelResultCode;
                }
                directories = directoryDialog.Value.Split(new [] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if(!directories.Any())
            {
                new MessageDialog("Error", "No directories to scan provided! Exiting.").Show();
                return ErrorResultCode;
            }

            var infobox = new Infobox("Scanning directories...");
            infobox.Show();

            Scanner.Instance.ScanDirectories(directories);

            var actions = new List<Tuple<string, string>> {
                Tuple.Create("All", "Generate solution file with all projects"),
                Tuple.Create("Custom", "Generate custom solution file"),
                Tuple.Create("Clean", "Remove generated configuration")
            };

            foreach(var uiType in Scanner.Instance.Elements.OfType<UiProject>().Select(x => x.UiType).Distinct().OrderByDescending(x => x))
            {
                actions.Insert(1, Tuple.Create(uiType, string.Format("Generate solution file for {0} with references", uiType)));
            }

            var actionDialog = new MenuDialog(Title, "Welcome to the Emul8 bootstrap configuration.\nUse this script to generate your own Emul8.sln file.\n\nChoose action:", actions);
            if(actionDialog.Show() != DialogResult.Ok)
            {
                return CancelResultCode;
            }

            Configuration configuration;
            try
            {
                var key = actionDialog.SelectedKeys.First();
                switch(key)
                {
                case "All":
                    configuration = GenerateAllProjects(binariesDirectory, generateEntryProject, directories);
                    break;
                case "Custom":
                    configuration = HandleCustomSolution(binariesDirectory, generateEntryProject, directories);
                    break;
                case "Clean":
                    Cleaner.Clean(outputDirectory);
                    new MessageDialog(Title, "Solution cleaned.").Show();
                    return CleanedResultCode;
                default:
                    var mainProject = Scanner.Instance.Elements.OfType<UiProject>().SingleOrDefault(x => x.UiType == key);
                    if(mainProject == null)
                    {
                        new MessageDialog("Bootstrap failure", string.Format("Could not load {0} project. Exiting", key)).Show();
                        return ErrorResultCode;
                    }
                    configuration = new Configuration(SolutionGenerator.GenerateWithAllReferences(mainProject, generateEntryProject, binariesDirectory), null);
                    break;
                }
            }
            catch(DirectoryNotFoundException e)
            {
                new MessageDialog("Error", e.Message).Show();
                return ErrorResultCode;
            }

            var confirmDialog = new YesNoDialog(Title, "Are you sure you want to create the solution file?");
            if(confirmDialog.Show() != DialogResult.Ok)
            {
                return CancelResultCode;
            }

            configuration.Save(outputDirectory);

            new MessageDialog(Title, "Solution file created successfully!").Show();
            return 0;
        }

        private static void HandleScan(ProjectType type, IEnumerable<string> directories)
        {
            if(directories == null || !directories.Any())
            {
                Console.Error.WriteLine("Don't know which folder to scan");
                return;
            }
            Scanner.Instance.ScanDirectories(directories);

            foreach(var p in Scanner.Instance.GetProjectsOfType(type))
            {
                Console.WriteLine("{0} {1}", p.Name, p.Path);
            }
        }

        private static Configuration GenerateAllProjects(string binariesPath, bool generateEntryProject, IEnumerable<string> paths)
        {
            var fullPaths = paths.Select(Path.GetFullPath);
            Scanner.Instance.ScanDirectories(fullPaths);

            var mainProject = Scanner.Instance.Elements.OfType<UiProject>().FirstOrDefault();
            if(mainProject == null)
            {
                Console.Error.WriteLine("No UI project found. Exiting");
                Environment.Exit(ErrorResultCode);
            }

            return new Configuration(
                SolutionGenerator.GenerateWithAllReferences(mainProject, generateEntryProject, binariesPath, 
                    Scanner.Instance.Elements.OfType<Project>().Where(x => !(x is UnknownProject))),
                Scanner.Instance.Elements.OfType<RobotTestSuite>());
        }

        private const int CancelResultCode = 1;
        private const int ErrorResultCode = 2;
        private const int CleanedResultCode = 3;

        public const string Title = "Emul8 bootstrap";
    }
}
