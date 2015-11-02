//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Text;
using Emul8.Core;

namespace Emul8.Logging
{
    public abstract class TextBackend : LoggerBackend
    {
        protected string FormatLogEntry(LogEntry entry)
        {
            var messageBuilder = new StringBuilder();
            var messages = entry.Message.Split('\n').GetEnumerator();
            messages.MoveNext();

            if(entry.ObjectName != null)
            {
                var currentEmulation = EmulationManager.Instance.CurrentEmulation;
                var machineCount = currentEmulation.MachinesCount;
                if(machineCount > 1 && entry.MachineName != null)
                {
                    messageBuilder.AppendFormat("{2}/{0}: {1}", entry.ObjectName, messages.Current, entry.MachineName);
                }
                else
                {
                    messageBuilder.AppendFormat("{0}: {1}", entry.ObjectName, messages.Current);
                }
            }
            else
            {
                messageBuilder.Append(messages.Current);
            }
            while(messages.MoveNext())
            {
                messageBuilder.Append(Environment.NewLine);
                messageBuilder.Append("    ");
                messageBuilder.Append(messages.Current);
            }
            return messageBuilder.ToString();
        }
    }
}

