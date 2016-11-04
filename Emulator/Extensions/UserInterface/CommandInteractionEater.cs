//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using AntShell.Commands;
using System.Text;

namespace Emul8.UserInterface
{
    public class CommandInteractionEater : ICommandInteraction
    {
        public void Clear()
        {
            data.Clear();
            error.Clear();
        }

        public void Write(char c, ConsoleColor? color = null)
        {
            if((c == 10) || (c == 13))
                return; // TODO
            data.Append(c);
        }

        public void WriteError(string error)
        {
            this.error.AppendLine(error);
        }

        public string ReadLine()
        {
            return String.Empty;
        }

        public string CommandToExecute { get; set; }

        public bool QuitEnvironment { get; set; }

        public string GetContents()
        {
            return data.ToString();
        }

        public string GetError()
        {
            return error.ToString();
        }

        private readonly StringBuilder data = new StringBuilder();
        private readonly StringBuilder error = new StringBuilder();
    }

    public class DummyCommandInteraction : ICommandInteraction
    {
        public DummyCommandInteraction(bool verbose = false)
        {
            this.verbose = verbose;
        }

        public string ReadLine()
        {
            return String.Empty;
        }

        public void Write(char c, ConsoleColor? color = default(ConsoleColor?))
        {
            if(verbose)
            {
                Console.Write(c);
            }
        }

        public void WriteError(string error)
        {
            ErrorDetected = true;
            if(verbose)
            {
                Console.WriteLine("ERROR: " + error);
            }
        }

        public bool ErrorDetected 
        { 
            get
            {
                var result = errorDetected;
                errorDetected = false;
                return result;
            }

            private set
            {
                errorDetected = value;
            }
        }

        public string CommandToExecute { get; set; }

        public bool QuitEnvironment { get; set; }

        private bool verbose;
        private bool errorDetected;
    }
}

