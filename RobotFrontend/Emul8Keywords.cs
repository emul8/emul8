//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using AntShell.Commands;
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
            interaction = new MemoryCommandInteraction();
            monitor = new Monitor();
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
        public string ExecuteCommand(string[] commands)
        {
            var command = string.Join(" ", commands);
            if(!monitor.Parse(command))
            {
                throw new KeywordException("Could not execute command '{0}': {1}", command, interaction.Error);
            }

            return interaction.Output;
        }

        [RobotFrameworkKeyword]
        public string ExecuteScript(string path)
        {
            if(!monitor.TryExecuteScript(path))
            {
                throw new KeywordException("Could not execute script: {0}", interaction.Error);
            }

            return interaction.Output;
        }

        [RobotFrameworkKeyword]
        public void StopRemoteServer()
        {
            RobotFrontend.Shutdown();
        }

        [RobotFrameworkKeyword]
        public void CreateAnalyzer(string peripheralName)
        {
            IUART uart;
            if(!monitor.Machine.TryGetByName(peripheralName, out uart))
            {
                throw new KeywordException("Peripheral not found or of wrong type: {0}", peripheralName);
            }

            tester = new TerminalTester(new TimeSpan(0, 0, 30));
            tester.Terminal.AttachTo(uart);
        }

        [RobotFrameworkKeyword]
        public void WaitForLine(string content)
        {
            tester.WaitUntilLine(x => x.Contains(content));
        }

        [RobotFrameworkKeyword]
        public void WaitForPrompt(string prompt)
        {
            tester.NowPromptIs(prompt);
            tester.WaitForPrompt();
        }

        [RobotFrameworkKeyword]
        public void WriteLine(string content)
        {
            tester.WriteLine(content);
        }

        private TerminalTester tester;
        private readonly Monitor monitor;
        private readonly MemoryCommandInteraction interaction;

        private class MemoryCommandInteraction : ICommandInteraction
        {
            public MemoryCommandInteraction()
            {
                output = new StringBuilder();
                error = new StringBuilder();
            }

            public string Output 
            { 
                get
                {
                    lock(output)
                    {
                        var result = output.ToString();
                        output.Clear();
                        return result;
                    }
                }
            }

            public string Error
            {
                get
                {
                    lock(error)
                    {
                        var result = error.ToString();
                        error.Clear();
                        return result;
                    }
                }
            }

            public string CommandToExecute { get; set; }

            public bool QuitEnvironment { get; set; }

            public string ReadLine()
            {
                throw new InvalidOperationException("Reading from memory command interaction is impossible");
            }

            public void Write(char c, ConsoleColor? color = default(ConsoleColor?))
            {
                lock(output)
                {
                    output.Append(c);
                }
            }

            public void WriteError(string errorMessage)
            {
                lock(error)
                {
                    error.Append(errorMessage);
                }
            }

            private readonly StringBuilder output;
            private readonly StringBuilder error;
        }
    }
}

