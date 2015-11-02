//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Emul8.Backends.Terminals;
using Emul8.Core;
using Mono.Linq.Expressions;
using Emul8.Logging;
using Emul8.Utilities;

namespace Emul8.Testing
{
    public static class TerminalTesterExtensions
    {
        public static void CreateTerminalTester(this Emulation emulation, string name, int timeoutInSeconds = 300, string prompt = @"/ # ")
        {
            var tester = new TerminalTester(TimeSpan.FromSeconds(timeoutInSeconds), prompt);
            emulation.ExternalsManager.AddExternal(tester, name);
        }
    }

    public class TerminalTester : BackendTerminal
    {
        public TerminalTester(TimeSpan globalTimeout, string prompt = @"/ # ")
        {
            timeoutSync = new object();
            eventCollection = new BlockingCollection<Event>(new ConcurrentQueue<Event>());
            reportCollection = new ConcurrentQueue<Event>();
            reportEndingLock = new object();
            reportCollection.Enqueue(new ReportSeparator());
            terminal = new PromptTerminal(x => eventCollection.Add(new Line { Content = x }), () => eventCollection.Add( new Prompt() ), prompt);
            defaultPrompt = prompt;
            this.globalTimeout = globalTimeout;
        }
        
        public TimeSpan GlobalTimeout
        {
            get
            {
                lock(timeoutSync)
                {
                    return globalTimeout;
                }
            }
            set
            {
                lock(timeoutSync)
                {
                    globalTimeout = value;
                }
            }
        }

        public override event Action<byte> CharReceived
        {
            add
            {
                Terminal.CharReceived += value;
            }
            remove
            {
                Terminal.CharReceived -= value;
            }
        }

        public override void WriteChar(byte value)
        {
            Terminal.WriteChar(value);
        }
        
        public BackendTerminal Terminal
        {
            get
            {
                return terminal;
            }
        }
        
        public void ClearReport()
        {
            reportCollection = new ConcurrentQueue<Event>();
        }
        
        public TerminalTester WaitUntilLine(Expression<Func<string, bool>> predicateExpression, TimeSpan? timeout = null)
        {
            var predicate = predicateExpression.Compile();
            WaitForEvent(x => 
            {
                var line = x as Line;
                if(line != null && predicate(line.Content))
                {
                    return EventResult.Success;
                }
                return EventResult.Continue;
            }, timeout, new Assertion(string.Format("WaitUntilLine({0})", predicateExpression.ToCSharpCode()))
                                     { Type = AssertionType.WaitUntilLine });
            return this;
        }
        
        public TerminalTester NowPromptIs(string prompt)
        {
            terminal.SetPrompt(prompt);
            return this;
        }

        public TerminalTester NowPromptIsDefault()
        {
            terminal.SetPrompt(defaultPrompt);
            return this;
        }
        
        public TerminalTester NextLine(Expression<Func<string, bool>> predicateExpression, TimeSpan? timeout = null)
        {
            var predicate = predicateExpression.Compile();
            var assertNotMet = false;
            var assertion = new Assertion(string.Format("NextLine({0})", predicateExpression.ToCSharpCode())) { Type = AssertionType.NextLine };
            WaitForEvent(x =>
            {
                var line = x as Line;
                if(line == null)
                {
                    return EventResult.Continue;
                }
                if(predicate(line.Content))
                {
                    return EventResult.Success;
                }
                ThreadPool.QueueUserWorkItem(y =>
                {
                    Thread.MemoryBarrier();
                    assertNotMet = true;
                    Thread.MemoryBarrier();
                });
                return EventResult.Failure; // line arrived, but not the proper one
            }, timeout, assertion);
            if(!assertNotMet)
            {
                return this;
            }
            lock(reportEndingLock)
            {
                reportCollection.Enqueue(new AssertNotMet(assertion));
                EndReport("Assert was not met");
            }
            throw new InvalidOperationException("Should not reach here.");
        }
        
        public TerminalTester WaitForPrompt(TimeSpan? timeout = null)
        {
            WaitForEvent(x => x is Prompt ? EventResult.Success : EventResult.Continue,
                        timeout, new Assertion { Type = AssertionType.WaitForPrompt });
            return this;
        }
        
        public TerminalTester WriteLine(string line = "", bool doNotEatEvent = false)
        {
            terminal.WriteLineToTerminal(line);
            var assertion = new Assertion(string.Format("WriteLine({0})", line)) { Type = AssertionType.WriteLine };
            if(!doNotEatEvent)
            {
                WaitForEvent(x => 
                {
                    var lineEvent = x as Line;
                    if(lineEvent == null)
                    {
                        return EventResult.Continue;
                    }
                    return lineEvent.Content.Contains(line) ? EventResult.Success : EventResult.Continue;
                },
                null, assertion);
            }
            return this;
        }
        
        public TerminalTester Write(string line)
        {
            terminal.WriteStringToTerminal(line);
            return this;
        }

        public TimeSpan WriteCharDelay
        {
            get
            {
                return terminal.WriteCharDelay;
            }
            set
            {
                terminal.WriteCharDelay = value;
            }
        }
        
        private void WaitForEvent(Func<Event, EventResult> predicate, TimeSpan? timeout, Assertion assertion)
        {
            TimeSpan usedTimeout;
            lock(timeoutSync)
            {
                usedTimeout = timeout ?? globalTimeout;
            }
            var done = new ManualResetEventSlim();
            var takeWaitCancellationSource = new CancellationTokenSource();
            var doneWaitCancellationSource = new CancellationTokenSource();
            try
            {
                ThreadPool.QueueUserWorkItem(x => 
                {
                    try
                    {
                        Event anEvent;
                        EventResult result;
                        do
                        {
                            anEvent = eventCollection.Take(takeWaitCancellationSource.Token);
                            reportCollection.Enqueue(anEvent);
                            result = predicate(anEvent);
                        }
                        while(result == EventResult.Continue);
                        if(result == EventResult.Success)
                        {
                            done.Set();
                        }
                        else
                        {
                            // we should cancel waiting for timeout
                            doneWaitCancellationSource.Cancel();
                        }
                    }
                    catch(ObjectDisposedException)
                    {
                        // not important anymore
                    }
                    catch(OperationCanceledException)
                    {
                        // timeout occured
                    }
                });
                try
                {
                    if(!done.Wait(usedTimeout, doneWaitCancellationSource.Token))
                    {
                        lock(reportEndingLock)
                        {
                            takeWaitCancellationSource.Cancel();
                            reportCollection.Enqueue(new Timeout(assertion));
                            if(assertion.Type == AssertionType.WaitForPrompt || assertion.Type == AssertionType.WaitUntilLine)
                            {
                                reportCollection.Enqueue(new WaitingChars { Chars = terminal.GetWaitingLine() });
                            }
                            EndReport(string.Format("Time for operation to finish has exceeded {0}", usedTimeout));
                        }
                    }
                    reportCollection.Enqueue(assertion);
                }
                catch(OperationCanceledException)
                {
                    // waiting cancelled, therefore no success, no event is added to report
                }
            }
            finally
            {
                done.Dispose();
                takeWaitCancellationSource.Dispose();
                doneWaitCancellationSource.Dispose();
            }
        }

        
        private void EndReport(string reason)
        {
            lock(reportEndingLock)
            {
                if(reportEnded)
                {
                    return;
                }
                reportEnded = true; // only first exception will reach the report
                reportCollection.Enqueue(new ReportSeparator());
                var events = reportCollection.Where(x => x.IsPrinted).Select(x => string.Format("[{0:HH:mm:ss.fff}]   {1}", x.Timestamp, x)).Aggregate((x, y) => x + "\n" + y);
                var message = string.Format("{0}. Events so far:\n{1}", reason, events);

                throw new InvalidOperationException(message);
            }
        }

        private readonly BlockingCollection<Event> eventCollection;
        private ConcurrentQueue<Event> reportCollection;
        private readonly PromptTerminal terminal;
        private TimeSpan globalTimeout;
        private readonly object reportEndingLock;
        private bool reportEnded;

        private enum EventResult
        {
            Success,
            Continue,
            Failure
        }
        
        private abstract class Event
        {
            public virtual bool IsPrinted
            {
                get
                {
                    return true;
                }
            }

            public readonly DateTime Timestamp = CustomDateTime.Now;
        }
        
        private class Prompt : Event
        {
            public override bool IsPrinted
            {
                get
                {
                    return false;
                }
            }
        }
        
        private class Line : Event
        {
            public string Content;
            
            public override string ToString()
            {
                return string.Format("\t{0}", Content);
            }
        }
        
        private class Assertion : Event
        {
            public Assertion(string assert = null)
            {
                this.assert = assert;
            }
            
            public string AssertText
            {
                get
                {
                    return assert;
                }
            }
            
            public AssertionType Type;
            
            public override string ToString()
            {
                return string.Format(">>> '{0}': success", assert ?? Type.ToString());
            }

            public override bool IsPrinted
            {
                get
                {
                    return Type != AssertionType.WriteLine;
                }
            }
            
            private readonly string assert;
        }
        
        private class Timeout : Event
        {
            public Timeout(Assertion assertion)
            {
                Assertion = assertion;
            }
            
            public Assertion Assertion { get; private set; }
            
            public override string ToString()
            {
                return string.Format(">>> '{0}': timeout occured", Assertion.AssertText ?? Assertion.Type.ToString());
            }
        }

        private class AssertNotMet : Event
        {
            public AssertNotMet(Assertion assertion)
            {
                Assertion = assertion;
            }
            
            public Assertion Assertion { get; private set; }

            public override string ToString()
            {
                 return string.Format(">>> '{0}': assert not met", Assertion.AssertText ?? Assertion.Type.ToString());
            }
        }
        
        private class WaitingChars : Event
        {
            public string Chars;
            
            public override string ToString()
            {
                return string.Format (">>> Characters waiting in buffer: \n\t{0}", Chars);
            }
        }
        
        private class ReportSeparator : Event
        {
            public override string ToString()
            {
                return "------------------------------------------------------------------------------------";
            }
        }

        private readonly object timeoutSync;
        private readonly string defaultPrompt;
        
        private enum AssertionType
        {
            WaitUntilLine,
            WaitForPrompt,
            NextLine,
            WriteLine
        }
    }
}

