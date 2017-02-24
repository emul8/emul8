using System;
using System.Linq;
using System.Collections.Generic;
using Emul8.Core;
using Microsoft.Scripting.Hosting;
using Emul8.Logging;
using Emul8.Time;

namespace Emul8.UserInterface
{
    public static class MonitorExecutorExtensions
    {
        public static void ExecutePythonEvery(this Machine machine, string name, int milliseconds, string script)
        {
            var engine = new ExecutorPythonEngine(machine, script);
            var clockEntry = new ClockEntry(milliseconds, ClockEntry.FrequencyToRatio(machine, 1000), engine.Action);
            machine.ObtainClockSource().AddClockEntry(clockEntry);

            if(events.TryAdd(machine, name, engine.Action))
            {
                machine.StateChanged += (m, s) => UnregisterEvent(m, name, s);
            }
        }

        public static void StopPythonExecution(this Machine machine, string name)
        {
            machine.ObtainClockSource().RemoveClockEntry(events.WithdrawAction(machine, name));
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
            public ExecutorPythonEngine(Machine machine, string script)
            {
                this.machine = machine;
                Scope.SetVariable("machine", machine);
                Scope.SetVariable("self", machine);

                source = new Lazy<ScriptSource>(() => Engine.CreateScriptSourceFromString(script));
                Action = () => source.Value.Execute(Scope);
            }

            public Action Action { get; private set; }

            private Lazy<ScriptSource> source;
            private readonly Machine machine;
        }

        private sealed class PeriodicEventsRegister
        {
            public bool TryAdd(Machine machine, string name, Action action)
            {
                lock(periodicEvents)
                {
                    if(HasEvent(machine, name))
                    {
                        machine.Log(LogLevel.Error, "Periodic event '{0}' already registered in this machine.", name);
                        return false;
                    }
                    periodicEvents.Add(Tuple.Create(machine, name, action));
                    return true;
                }
            }

            public Action WithdrawAction(Machine machine, string name)
            {
                lock(periodicEvents)
                {
                    var action = periodicEvents.Single(x => x.Item1 == machine && x.Item2 == name).Item3;
                    Remove(machine, name);
                    return action;
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

            private static readonly List<Tuple<Machine, string, Action>> periodicEvents = new List<Tuple<Machine, string, Action>>();
        }
    }
}
