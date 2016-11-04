//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
﻿using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Emul8.Bootstrap
{
    public abstract class Dialog
    {
        protected Dialog(string title, string message)
        {
            this.title = title;
            this.message = message;
        }
        
        public abstract DialogResult Show();

        protected DialogResult Show(string arguments)
        {
            var psi = new ProcessStartInfo("dialog", arguments);
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;

            var process = Process.Start(psi);
            process.WaitForExit();
            output = process.StandardError.ReadToEnd();
            if(output.StartsWith("\"") && output.EndsWith("\""))
            {
                // if output from dialog is put in quotes it needs unescaping
                output = output.Substring(1, output.Length - 2);
                output = Regex.Replace(output, @"\(.)", "$1");
            }
            return (DialogResult) process.ExitCode;
        }

        protected string output;

        protected readonly string title;
        protected readonly string message;
    }
}

