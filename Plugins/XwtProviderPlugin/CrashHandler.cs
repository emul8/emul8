//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.IO;
using Xwt;
using System.Text;
using System.Threading;
using System.Linq;
using Emul8.Utilities;
using Emul8.Plugins.XwtProviderPlugin;

namespace Emul8.Plugins.XwtProviderPlugin
{
    public static class CrashHandler
    {
        public static void HandleCrash(Exception e)
        {
            var path = TemporaryFilesManager.Instance.EmulatorTemporaryPath + TemporaryFilesManager.CrashSuffix;
            Directory.CreateDirectory(path);
            var filename = CustomDateTime.Now.ToString("yyyyMMddHHmmssfff");
            var ex = e;
            using(var file = File.CreateText(Path.Combine(path, filename)))
            {
                while(ex != null)
                {
                    file.WriteLine(ex.Message);
                    file.WriteLine(ex.StackTrace);
                    ex = ex.InnerException;
                    if(ex != null)
                    {
                        file.WriteLine("Inner exception:");
                    }
                }
            }

            ex = e;
            var dialog = new Dialog();
            dialog.Title = "Fatal error";
            var sb = new StringBuilder();
            while(ex != null)
            {
                sb.AppendLine(ex.Message);
                #if DEBUG
                sb.AppendLine(ex.StackTrace);
                #endif
                ex = ex.InnerException;
                if(ex != null)
                {
                    sb.AppendLine("Inner exception:");
                }
            }

            var markdown = new MarkdownView();
            markdown.Markdown = sb.ToString().Split(new [] { '\n' }).Select(x => "\t" + x).Aggregate((x, y) => x + "\n" + y);

            var copyButton = new Button("Copy to clipboard");
            copyButton.Clicked += (sender, ev) => Clipboard.SetText(sb.ToString());

            var box = new VBox();

            box.PackStart(new Label( String.Format("Got unhandled exception: `{0}`", e.GetType()) ) { Font = global::Xwt.Drawing.Font.SystemFont.WithSize(15).WithWeight(Xwt.Drawing.FontWeight.Bold) });
            box.PackStart(new ScrollView(markdown), true, true);
            box.PackStart(copyButton);

            dialog.Content = box;

            dialog.Buttons.Add(new DialogButton(Command.Ok));
            dialog.Width = 350;
            dialog.Height = 300;

            var mre = new ManualResetEvent(false);
            Console.WriteLine(sb);

            ApplicationExtensions.InvokeInUIThread(() => {
                dialog.Run();
                dialog.Dispose();

                mre.Set();
            });

            mre.WaitOne();

            Environment.Exit(-1);
        }
    }
}
