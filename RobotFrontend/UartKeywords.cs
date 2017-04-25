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
        public void CreateTerminalTester(string uart, string prompt = null, int timeout = 30, string machine = null)
        {
            lock(testers)
            {
                if(testers.ContainsKey(uart))
                {
                    throw new KeywordException("Terminal tester for peripheral {0} already exists", uart);
                }

                Machine machineObject;
                if(machine == null)
                {
                    if(!EmulationManager.Instance.CurrentEmulation.Machines.Any())
                    {
                        throw new KeywordException("There is no machine in the emulation. Could not create tester for peripheral: {0}", uart);
                    }
                    machineObject = EmulationManager.Instance.CurrentEmulation.Machines.Count() == 1
                        ? EmulationManager.Instance.CurrentEmulation.Machines.First()
                        : null;
                    if(machineObject == null)
                    {
                        throw new KeywordException("No machine name provided. Don't know which one to choose.");
                    }
                }
                else if(!EmulationManager.Instance.CurrentEmulation.TryGetMachineByName(machine, out machineObject))
                {
                    throw new KeywordException("Machine with name {0} not found.", machine);
                }

                IUART uartObject;
                if(!machineObject.TryGetByName(uart, out uartObject))
                {
                    throw new KeywordException("Peripheral not found or of wrong type: {0}", uart);
                }

                var tester = new TerminalTester(TimeSpan.FromSeconds(timeout), prompt);
                tester.Terminal.AttachTo(uartObject);
                testers.Add(uart, tester);
            }
        }

        [RobotFrameworkKeyword]
        public void SetNewPromptForUart(string prompt, string uart = null)
        {
            GetTesterOrThrowException(uart).NowPromptIs(prompt);
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
        public double GetVirtualTimestampOfLastEvent(string uart = null)
        {
            return GetTesterOrThrowException(uart).LastEventVirtualTimestamp.TotalMilliseconds;
        }
   
        [RobotFrameworkKeyword]
        public void TestIfUartIsIdle(int timeInSeconds, string uart = null)
        {
            GetTesterOrThrowException(uart).CheckIfUartIsIdle(TimeSpan.FromSeconds(timeInSeconds));
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
