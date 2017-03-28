//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Peripherals.UART;
using Emul8.Robot;
using Emul8.Testing;
using Emul8.Core;
using System.Linq;

namespace Emul8.RobotFrontend
{
    internal class UartKeywords : IRobotFrameworkKeywordProvider
    {
        public UartKeywords()
        {
            testers = new Dictionary<string, TerminalTester>();
            EmulationManager.Instance.EmulationChanged += () =>  
            { 
                lock(testers) 
                { 
                    testers.Clear(); 
                }
            };
        }

        public void Dispose()
        {
        }

        [RobotFrameworkKeyword]
        public void CreateTerminalTester(string machineName, string peripheralName, string prompt, int timeout = 30)
        {
            lock(testers)
            {
                if(testers.ContainsKey(peripheralName))
                {
                    throw new KeywordException("Terminal tester for peripheral {0} already exists");
                }

                Machine machine;
                if(machineName == null)
                {
                    if(!EmulationManager.Instance.CurrentEmulation.Machines.Any())
                    {
                        throw new KeywordException("There is no machine in the emulation. Could not create tester for peripheral: {0}", peripheralName);
                    }
                    machine = EmulationManager.Instance.CurrentEmulation.Machines.SingleOrDefault();
                    if(machine == null)
                    {
                        throw new KeywordException("No machine name provided. Don't know which one to choose.");
                    }
                }
                else if(!EmulationManager.Instance.CurrentEmulation.TryGetMachineByName(machineName, out machine))
                {
                    throw new KeywordException("Machine with name {0} not found.");
                }

                IUART uart;
                if(!machine.TryGetByName(peripheralName, out uart))
                {
                    throw new KeywordException("Peripheral not found or of wrong type: {0}", peripheralName);
                }

                var tester = new TerminalTester(TimeSpan.FromSeconds(timeout), prompt);
                tester.Terminal.AttachTo(uart);
                testers.Add(peripheralName, tester);
            }
        }


        [RobotFrameworkKeyword]
        public void CreateTerminalTester(string peripheralName, string prompt, int timeout = 30)
        {
            CreateTerminalTester(null, peripheralName, prompt, timeout);
        }

        [RobotFrameworkKeyword]
        public void SetNewPromptForUart(string peripheralName, string prompt)
        {
            GetTesterOrThrowException(peripheralName).NowPromptIs(prompt);
        }

        [RobotFrameworkKeyword]
        public void SetNewPromptForUart(string prompt) 
        {
            SetNewPromptForUart(null, prompt);
        }

        [RobotFrameworkKeyword]
        public void WaitForLineOnUart(string content, int? timeout = null)
        {
            WaitForLineOnUart(null, content, timeout);
        }

        [RobotFrameworkKeyword]
        public void WaitForLineOnUart(string peripheralName, string content, int? timeout = null)
        {
            GetTesterOrThrowException(peripheralName).WaitUntilLine(x => x.Contains(content), timeout == null ? (TimeSpan?)null : TimeSpan.FromSeconds(timeout.Value));
        }

        [RobotFrameworkKeyword]
        public string WaitForPromptOnUart(string peripheralName = null, int? timeout = null)
        {
            return GetTesterOrThrowException(peripheralName).ReadToPrompt(timeout == null ? (TimeSpan?)null : TimeSpan.FromSeconds(timeout.Value));
        }

        [RobotFrameworkKeyword]
        public void WriteLineToUart(string content)
        {
            WriteLineToUart(null, content);
        }

        [RobotFrameworkKeyword]
        public void WriteLineToUart(string peripheralName, string content)
        {
            GetTesterOrThrowException(peripheralName).WriteLine(content);
        }

        [RobotFrameworkKeyword]
        public double GetLastEventVirtualTimestamp(string peripheralName = null)
        {
            return GetTesterOrThrowException(peripheralName).LastEventVirtualTimestamp.TotalMilliseconds;
        }

        private TerminalTester GetTesterOrThrowException(string peripheralName)
        {
            lock(testers)
            {
                TerminalTester tester;
                if(peripheralName == null)
                {
                    if(testers.Count != 1)
                    {
                        throw new KeywordException("There are more than one terminal tester available - please specify the uart name.");
                    }
                    tester = testers.Single().Value;
                }
                else if(!testers.TryGetValue(peripheralName, out tester))
                {
                    throw new KeywordException("Terminal tester for peripheral {0} not found. Did you forget to call `Create Terminal Tester`?", peripheralName);
                }
                return tester;
            }
         }

        private readonly Dictionary<string, TerminalTester> testers;
    }
}
