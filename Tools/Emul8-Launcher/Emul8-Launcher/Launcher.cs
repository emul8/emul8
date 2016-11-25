//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Diagnostics;
using Antmicro.OptionsParser;
using System.Collections.Generic;
using System.Linq;

namespace Emul8.Launcher
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            var basicOptions = new BasicOptions();
            var optionsParser = new OptionsParser(new ParserConfiguration
            {
                GenerateHelp = false,
                AllowUnexpectedArguments = true
            });

            // here we parse only one option - `root-directory`
            // the rest will be parsed in the second phase
            if(!optionsParser.Parse(basicOptions, args))
            {
                return;
            }

            optionsParser = new OptionsParser(new ParserConfiguration
            {
                GenerateHelp = true,
                CustomFooterGenerator = () => "All unparsed options will be forwarded to launched application.",
                CustomUsageLineGenerator = usageLine => usageLine + " [script]",
                AllowUnexpectedArguments = true
            });

            var addHelpSwitch = false;
            var possibleLaunchees = new List<LaunchDescriptorsGroup>();
            var selectedLaunchees = new List<LaunchDescriptorsGroup>();

            var interestingBinaries = Scanner.ScanForInterestingBinaries(basicOptions.RootPath).OrderBy(x => x.Priority).ThenBy(x => x.Name).ToArray();
            for(var i = 0; i < interestingBinaries.Length; i++) 
            {
                var j = i;
                possibleLaunchees.Add(interestingBinaries[i]);
                interestingBinaries[i].GenerateSwitches(optionsParser, i == 0);
                interestingBinaries[i].SwitchOption.Parsed += (option, value) =>
                {
                    selectedLaunchees.Add(interestingBinaries[j]);
                };

                if(interestingBinaries[i].HelpOption != null)
                {
                    interestingBinaries[i].HelpOption.Parsed += (option, value) =>
                    {
                        addHelpSwitch = true;
                    };
                }
            }

            // here we parse all of the options again
            // as only now we know which launch targets
            // and configurations are available in the
            // system
            var options = new Options();
            if(!optionsParser.Parse(options, args))
            {
                return;
            }

            var conf = options.Debug ? LaunchDescriptor.Configuration.Debug : LaunchDescriptor.Configuration.Release;
            LaunchDescriptorsGroup selectedGroup = null;
            LaunchDescriptor selectedLaunchee = null;
            if(selectedLaunchees.Count == 0)
            {
                if(possibleLaunchees.Count == 0)
                {
                    Console.Error.WriteLine("There is no launch target specified. Exiting");
                    Environment.Exit(1);
                }

                selectedGroup = possibleLaunchees.OrderBy(x => x.Priority).ThenBy(x => x.Name).First();
            }
            else if(selectedLaunchees.Count > 1)
            {
                Console.Error.WriteLine("Only one launch target can be used. Exiting");
                Environment.Exit(1);
            }
            else
            {
                selectedGroup = selectedLaunchees.First();
            }

            selectedLaunchee = selectedGroup.ForConfiguration(conf);
            if(selectedLaunchee == null)
            {
                Console.Error.WriteLine("Selected target {0} is not available in {1} configuration.", selectedGroup.Name, conf);
                Environment.Exit(1);
            }

#if !EMUL8_PLATFORM_WINDOWS
	    // it is not so easy to remotely debug on .NET, so we do not support it
            var monoOptions = options.Debug ? "--debug" : string.Empty;

            if(options.DebuggerSocketPort != -1)
            {
                monoOptions += string.Format(" --debugger-agent=transport=dt_socket,address=127.0.0.1:{0},server=y", options.DebuggerSocketPort);
                Console.WriteLine("Listening on port {0}", options.DebuggerSocketPort);
            }
#endif

            var optionsToPass = optionsParser.RecreateUnparsedArguments();
            if(addHelpSwitch)
            {
                Console.WriteLine("NOTE: showing help from binary: {0}", selectedLaunchee.Name);
                optionsToPass = "--help";
            }

            var process = new Process();
#if EMUL8_PLATFORM_WINDOWS
            process.StartInfo.FileName = selectedLaunchee.Path;
            process.StartInfo.Arguments = optionsToPass;
#else
            process.StartInfo.FileName = "mono";
            process.StartInfo.Arguments = string.Format("{0} {1} {2}", monoOptions, selectedLaunchee.Path, optionsToPass);
#endif

            if(options.Quiet)
            {
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
            }

            process.Start();
            if(options.Quiet)
            {
                process.StandardError.Close();
            }

            process.WaitForExit();
        }
    }
}
