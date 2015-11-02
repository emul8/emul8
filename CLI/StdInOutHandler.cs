//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System.Text;
using AntShell;
using AntShell.Terminal;
using Emul8.UserInterface;

namespace Emul8.CLI 
{
    public class StdInOutHandler
    {
        public StdInOutHandler(DetachableIO io, Monitor monitor, bool plainOutput = false)
        {
            this.io = io;
            commandHandler = monitor;

            buffer = new StringBuilder();
            CommandInteraction = new StdInOutInteraction(plainOutput);
            monitor.Interaction = CommandInteraction;
        }

        public void Start()
        {
            CommandInteraction.CharReceived += io.Write;
            io.ByteRead += HandleByte;
        }
      
        private void HandleByte(byte b)
        {
            if (b == '\n' || b == ';')
            {
                // execute command and clear buffer
                commandHandler.HandleCommand(buffer.ToString(), CommandInteraction);
                buffer.Clear();
            }
            else
            {
                buffer.Append((char)b);
            }
        }

        private readonly StringBuilder buffer;
        private readonly ICommandHandler commandHandler;
        private readonly DetachableIO io;
        private readonly StdInOutInteraction CommandInteraction;
    }
}

