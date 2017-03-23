//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Robot;
using Emul8.UserInterface;
using Emul8.Utilities;

namespace Emul8.RobotFrontend
{
    internal class Emul8Keywords : IRobotFrameworkKeywordProvider
    {
        public Emul8Keywords()
        {
            monitor = new Monitor { Interaction = new CommandInteractionEater() };
        }

        public void Dispose()
        {
        }

        [RobotFrameworkKeyword]
        public void ResetEmulation()
        {
            EmulationManager.Instance.Clear();
            Recorder.Instance.ClearEvents();
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
            var interaction = monitor.Interaction as CommandInteractionEater;
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
            var interaction = monitor.Interaction as CommandInteractionEater;
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
        public void HandleHotSpot(HotSpotAction action)
        {
            switch(action)
            {
                case HotSpotAction.None:
                    // do nothing
                    break;
                case HotSpotAction.Pause:
                    EmulationManager.Instance.CurrentEmulation.PauseAll();
                    EmulationManager.Instance.CurrentEmulation.StartAll();
                    break;
                case HotSpotAction.Serialize:
                    var fileName = TemporaryFilesManager.Instance.GetTemporaryFile();
                    EmulationManager.Instance.Save(fileName);
                    EmulationManager.Instance.Load(fileName);
                    EmulationManager.Instance.CurrentEmulation.StartAll();
                    break;
                default:
                    throw new KeywordException("Hot spot action {0} is not currently supported", action);
            }
        }

        [RobotFrameworkKeyword]
        public void Provides(string state)
        {
            Recorder.Instance.SaveCurrentState(state);
        }

        [RobotFrameworkKeyword]
        public void Requires(string state)
        {
            List<Recorder.Event> events;
            if(!Recorder.Instance.TryGetState(state, out events))
            {
                throw new KeywordException("Required state {0} not found.", state);
            }
            ResetEmulation();
            foreach(var e in events)
            {
                RobotFrontend.ExecuteKeyword(e.Name, e.Arguments);
            }
        }

        private readonly Monitor monitor;
    }
}

