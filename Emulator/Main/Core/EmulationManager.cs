//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Antmicro.Migrant;
using System.IO;
using System;
using Emul8.Exceptions;
using IronPython.Runtime;
using Emul8.Peripherals.Python;
using Emul8.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using Emul8.Logging;

namespace Emul8.Core
{
    public sealed class EmulationManager
    {
        public static EmulationManager Instance { get; private set; }

        static EmulationManager()
        {
            Reset();
        }

        public static void Reset()
        {
            Instance = new EmulationManager();
        }

        public ProgressMonitor ProgressMonitor { get; private set; }

        public Emulation CurrentEmulation
        { 
            get
            {
                return currentEmulation;
            }
            set
            {
                var oldEmulation = Interlocked.Exchange(ref currentEmulation, value);
                oldEmulation.Dispose();
                InvokeEmulationChanged();
            }
        }

        public void Load(string path)
        {
            string version;
            using(var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                version = serializer.Deserialize<string>(stream);
                CurrentEmulation = serializer.Deserialize<Emulation>(stream);
                CurrentEmulation.BlobManager.Load(stream);
            }

            if(version != VersionString)
            {
                Logger.Log(LogLevel.Warning, "Version of deserialized emulation ({0}) does not match current one {1}. Things may go awry!", version, VersionString);
            }
        }

        public void Save(string path)
        {
            try
            {
                using(var stream = new FileStream(path, FileMode.Create))
                {
                    using(CurrentEmulation.ObtainPausedState())
                    {
                        try
                        {
                            serializer.Serialize(VersionString, stream);
                            serializer.Serialize(CurrentEmulation, stream);
                            CurrentEmulation.BlobManager.Save(stream);
                        }
                        catch(InvalidOperationException e)
                        {
                            throw new RecoverableException(string.Format("Error encountered during saving: {0}.", e.Message));
                        }
                    }
                }
            }
            catch(Exception)
            {
                File.Delete(path);
                throw;
            }
        }

        public void Clear()
        {
            CurrentEmulation = new Emulation();
        }

        public TimerResult StartTimer(string eventName = null)
        {
            stopwatch.Reset();
            stopwatchCounter = 0;
            var timerResult = new TimerResult {
                FromBeginning = TimeSpan.FromTicks(0),
                SequenceNumber = stopwatchCounter,
                Timestamp = CustomDateTime.Now,
                EventName = eventName
            };
            stopwatch.Start();
            return timerResult;
        }

        public TimerResult CurrentTimer(string eventName = null)
        {
            stopwatchCounter++;
            return new TimerResult {
                FromBeginning = stopwatch.Elapsed,
                SequenceNumber = stopwatchCounter,
                Timestamp = CustomDateTime.Now,
                EventName = eventName
            };
        }

        public TimerResult StopTimer(string eventName = null)
        {
            stopwatchCounter++;
            var timerResult = new TimerResult {
                FromBeginning = stopwatch.Elapsed,
                SequenceNumber = stopwatchCounter,
                Timestamp = CustomDateTime.Now,
                EventName = eventName
            };
            stopwatch.Stop();
            stopwatch.Reset();
            return timerResult;
        }

        public string VersionString
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var name = ((AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
                var version = assembly.GetName().Version;
                var gitVersion = ((AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0]).InformationalVersion;
                return string.Format("{0}, version {1} ({2})", name, version, gitVersion);
            }
        }

        public event Action EmulationChanged;

        private EmulationManager()
        {
            var settings = new Antmicro.Migrant.Customization.Settings(Antmicro.Migrant.Customization.Method.Generated, Antmicro.Migrant.Customization.Method.Generated,
                Antmicro.Migrant.Customization.VersionToleranceLevel.AllowGuidChange, disableTypeStamping: true);
            serializer = new Serializer(settings);
            serializer.ForObject<PythonDictionary>().SetSurrogate(x => new PythonDictionarySurrogate(x));
            serializer.ForSurrogate<PythonDictionarySurrogate>().SetObject(x => x.Restore());
            currentEmulation = new Emulation();
            ProgressMonitor = new ProgressMonitor();
            stopwatch = new Stopwatch();
        }

        private void InvokeEmulationChanged()
        {
            var emulationChanged = EmulationChanged;
            if(emulationChanged != null)
            {
                emulationChanged();
            }
        }

        private int stopwatchCounter;
        private Stopwatch stopwatch;
        private readonly Serializer serializer;
        private Emulation currentEmulation;
    }
}

