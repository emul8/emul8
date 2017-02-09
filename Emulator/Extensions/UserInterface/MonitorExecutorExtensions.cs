using System;
using System.Linq;
using System.Collections.Generic;
using Emul8.Core;
using Emul8.Exceptions;
using Microsoft.Scripting.Hosting;
using Emul8.Logging;

namespace Emul8.UserInterface
{
    public static class MonitorExecutorExtensions
    {
        public static void ExecutePythonEvery(this Machine machine, string name, int seconds, string script)
        {
            var engine = new ExecutorPythonEngine(machine, name, script, seconds);
            machine.StateChanged += (m, s) => UnregisterEvent(m, name, s);
            try
            {
                engine.AddAction();
                events.Add(machine, name);
            }
            catch(InvalidOperationException e)
            {
                throw new RecoverableException(e);
            }
        }

        public static void StopPythonExecution(this Machine machine, string name)
        {
            events.Remove(machine, name);
        }

        private static void UnregisterEvent(Machine machine, String name, MachineStateChangedEventArgs state)
        {
            if(state.CurrentState == MachineStateChangedEventArgs.State.Disposed)
            {
                events.Remove(machine, name);
            }
        }

        private static readonly PeriodicEventsRegister events = new PeriodicEventsRegister();

        private sealed class ExecutorPythonEngine : PythonEngine
        {
            public ExecutorPythonEngine(Machine machine, string name, string script, int seconds)
            {
                this.machine = machine;
                this.timeout = TimeSpan.FromSeconds(seconds);
                Scope.SetVariable("machine", machine);
                Scope.SetVariable("self", machine);
                source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(script));
                Action = () =>
                {
                    if(events.HasEvent(machine, name))
                    {
                        source.Value.Execute(Scope);
                        AddAction();
                    }
                };
            }

            public void AddAction()
            {
                machine.ExecuteIn(Action, timeout);
            }

            public Action Action { get; private set; }

            private TimeSpan timeout;
            private Lazy<ScriptSource> source;
            private readonly Machine machine;
        }

        private sealed class PeriodicEventsRegister
        {
            public void Add(Machine machine, String name)
            {
                lock(periodicEvents)
                {
                    if(HasEvent(machine, name))
                    {
                        machine.Log(LogLevel.Error, "Periodic event '{0}' already registered in this machine.", name);
                    }
                    periodicEvents.Add(Tuple.Create(machine, name));
                }
            }

            public void Remove(Machine machine, String name)
            {
                lock(periodicEvents)
                {
                    periodicEvents.RemoveAll(x => x.Item1 == machine && x.Item2 == name);
                }
            }

            public bool HasEvent(Machine machine, String name)
            {
                lock(periodicEvents)
                {
                    return periodicEvents.Any(x => x.Item1 == machine && x.Item2 == name);
                }
            }

            private static readonly List<Tuple<Machine, String>> periodicEvents = new List<Tuple<Machine, string>>();
        }
    }
}
