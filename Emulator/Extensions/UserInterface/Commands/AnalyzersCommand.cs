//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using Emul8.UserInterface.Tokenizer;
using Emul8.Peripherals;
using Emul8.Core;
using Emul8.Exceptions;

namespace Emul8.UserInterface.Commands
{
    public class AnalyzersCommand : AutoLoadCommand
    {
        public override void PrintHelp(AntShell.Commands.ICommandInteraction writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine("------");
            writer.WriteLine("analyzers [peripheral]");
            writer.WriteLine("\tlists ids of available analyzer for [peripheral]");
            writer.WriteLine("");
            writer.WriteLine("analyzers default [peripheral]");
            writer.WriteLine("\twrites id of default analyzer for [peripheral]");
        }

        [Runnable]
        public void Run(ICommandInteraction writer, LiteralToken peripheralName)
        {
            var emu = EmulationManager.Instance.CurrentEmulation;
            IPeripheral p;

            try
            {
                p = (IPeripheral)monitor.ConvertValueOrThrowRecoverable(peripheralName.Value, typeof(IPeripheral));
            }
            catch(RecoverableException)
            {
                writer.WriteError(string.Format("Peripheral not found: {0}", peripheralName.Value));
                return;
            }

            IAnalyzableBackend backend;
            if(!emu.BackendManager.TryGetBackendFor(p, out backend))
            {
                writer.WriteError(string.Format("No backend found for {0}", peripheralName.Value));
                return;
            }

            foreach(var a in emu.BackendManager.GetAvailableAnalyzersFor(backend))
            {
                writer.WriteLine(a);
            }
        }

        [Runnable]
        public void Run(ICommandInteraction writer, [Values("default")] LiteralToken @default, LiteralToken peripheralName)
        {
            var emu = EmulationManager.Instance.CurrentEmulation;
            IPeripheral p;
            string fake;

            var m = monitor.Machine;
            if(m == null || !m.TryGetByName(peripheralName.Value, out p, out fake))
            {
                writer.WriteError(string.Format("Peripheral not found: {0}", peripheralName.Value));
                return;
            }

            IAnalyzableBackend backend;
            if(!emu.BackendManager.TryGetBackendFor(p, out backend))
            {
                writer.WriteError(string.Format("No backend found for {0}", peripheralName.Value));
                return;
            }

            var def = emu.BackendManager.GetPreferredAnalyzerFor(backend);
            writer.WriteLine(def ?? "No default analyzer found.");
        }

        public AnalyzersCommand(Monitor monitor) : base(monitor, "analyzers", "shows available analyzers for peripheral.")
        {
        }
    }
}

