//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System;
using System.IO;
using System.Diagnostics;
using Antmicro.OptionsParser;

namespace Emul8Launcher
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            var parser = new OptionsParser(new ParserConfiguration 
            { 
                GenerateHelp = true,
                CustomFooterGenerator = () => "All unparsed options will be forwarded to CLI application.",
                CustomUsageLineGenerator = usageLine => usageLine + " [script]",
                AllowUnexpectedArguments = true
            });
            
            var options = new Options();
            if(!parser.Parse(options, args))
            {
                return;
            }
            
            var mode = options.Debug ? "Debug" : "Release";
            var monoOptions = options.Debug ? "--debug" : string.Empty;
            
            if(options.DebuggerSocketPort != -1)
            {
                monoOptions += string.Format(" --debugger-agent=transport=dt_socket,address=127.0.0.1:{0},server=y", options.DebuggerSocketPort);
                Console.WriteLine("Listening on port {0}", options.DebuggerSocketPort);
            }
            
            var rootPath = (options.RootPath == ".") ? Environment.CurrentDirectory : options.RootPath;
            Console.WriteLine("Running in {0} mode.\nROOT_PATH={1}", mode, rootPath);
            
            var binaryPath = Path.Combine(rootPath, "output", mode, "CLI.exe");
            var optionsToPass = parser.RecreateUnparsedArguments();
            if(options.HelpCLI)
            {
                Console.WriteLine("NOTE: showing help from binary: {0}", binaryPath);
                optionsToPass = "--help";
            }
            
            var process = new Process();
            process.StartInfo.FileName = "mono";
            process.StartInfo.Arguments = string.Format("{0} {1} {2}", monoOptions, binaryPath, optionsToPass);
            
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
