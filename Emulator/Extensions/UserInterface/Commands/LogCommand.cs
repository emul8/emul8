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
using Emul8.Logging;

namespace Emul8.UserInterface.Commands
{
    public class LogCommand : AutoLoadCommand
    {
        public override void PrintHelp(ICommandInteraction writer)
        {
            base.PrintHelp(writer);
            writer.WriteLine();
            writer.WriteLine("Usage:");
            writer.WriteLine(String.Format("{0} <<message to log>> <<log level>>", Name));
        }
        [Runnable]
        public void Run(ICommandInteraction writer, StringToken message)
        {
            InnerLog(LogLevel.Debug, message.Value);
        }

        [Runnable]
        public void Run(ICommandInteraction writer, StringToken message, [Values( -1L, 0L, 1L, 2L, 3L)] NumericToken level)
        {
            InnerLog((LogLevel)(int)level.Value, message.Value);
        }

        [Runnable]
        public void Run(ICommandInteraction writer, StringToken message, [Values( "Noisy", "Debug", "Info", "Warning", "Error")] StringToken level)
        {
            InnerLog(LogLevel.Parse(level.Value), message.Value);
        }

        private void InnerLog(LogLevel logLevel, string message)
        {
            Logger.LogAs(monitor, logLevel, "Script: " + message);
        }

        public LogCommand(Monitor monitor)
            : base(monitor, "log", "logs messages.")
        {
        }
    }
}

