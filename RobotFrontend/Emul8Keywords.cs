//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Peripherals.UART;
using Emul8.Robot;
using Emul8.Testing;
using Emul8.UserInterface;

namespace Emul8.RobotFrontend
{
    internal class Emul8Keywords : IRobotFrameworkKeywordProvider
    {
        public Emul8Keywords()
        {
            interaction = new CommandInteractionEater();
            monitor = new Monitor();
            testers = new Dictionary<string, TerminalTester>();
            monitor.Interaction = interaction;
        }

        public void Dispose()
        {
        }

        [RobotFrameworkKeyword]
        public void ResetEmulation()
        {
            EmulationManager.Instance.Clear();
        }

        [RobotFrameworkKeyword]
        public void StartEmulation()
        {
            EmulationManager.Instance.CurrentEmulation.StartAll();
        }

        [RobotFrameworkKeyword]
        // This method accepts array of strings that is later
        // concatenated using single space and parsed by the monitor.
        //
        // Using array instead of a single string allows us to
        // split long commands into several lines using (...)
        // notation in robot script; otherwise it would be impossible
        // as there is no option to split a single parameter.
        public string ExecuteCommand(string[] commandFragments)
        {
            interaction.Clear();
            var command = string.Join(" ", commandFragments);
            if(!monitor.Parse(command))
            {
                throw new KeywordException("Could not execute command '{0}': {1}", command, interaction.GetError());
            }

            return interaction.GetContents();
        }

        [RobotFrameworkKeyword]
        public string ExecuteScript(string path)
        {
            interaction.Clear();
            
            if(!monitor.TryExecuteScript(path))
            {
                throw new KeywordException("Could not execute script: {0}", interaction.GetError());
            }

            return interaction.GetContents();
        }

        [RobotFrameworkKeyword]
        public void StopRemoteServer()
        {
            RobotFrontend.Shutdown();
        }

        [RobotFrameworkKeyword]
        public void CreateTerminalTester(string peripheralName)
        {
            if(testers.ContainsKey(peripheralName))
            {
                throw new KeywordException("Terminal tester for peripheral {0} already exists");
            }
            
            IUART uart;
            if(!monitor.Machine.TryGetByName(peripheralName, out uart))
            {
                throw new KeywordException("Peripheral not found or of wrong type: {0}", peripheralName);
            }

            var tester = new TerminalTester(new TimeSpan(0, 0, 30));
            tester.Terminal.AttachTo(uart);

            testers.Add(peripheralName, tester);
        }

        [RobotFrameworkKeyword]
        public void WaitForLine(string peripheralName, string content)
        {
            GetTesterOrThrowException(peripheralName).WaitUntilLine(x => x.Contains(content));
        }

        [RobotFrameworkKeyword]
        public void WaitForPrompt(string peripheralName, string prompt)
        {
            var tester = GetTesterOrThrowException(peripheralName);
            tester.NowPromptIs(prompt);
            tester.WaitForPrompt();
        }

        [RobotFrameworkKeyword]
        public void WriteLine(string peripheralName, string content)
        {
            GetTesterOrThrowException(peripheralName).WriteLine(content);
        }

        private TerminalTester GetTesterOrThrowException(string peripheralName)
        {
            TerminalTester tester;
            if(!testers.TryGetValue(peripheralName, out tester))
            {
                throw new KeywordException("Terminal tester for peripheral {0} not found. Did you forget to call `CreateTerminalTester`?", peripheralName);
            }
            return tester;
         }

        private readonly Dictionary<string, TerminalTester> testers;
        private readonly Monitor monitor;
        private readonly CommandInteractionEater interaction;
    }
}

