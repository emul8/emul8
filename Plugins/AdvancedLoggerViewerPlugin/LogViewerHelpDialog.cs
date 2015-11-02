//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Xwt;
using System.IO;

namespace Emul8.Plugins.AdvancedLoggerViewer
{
    public class LogViewerHelpDialog : Dialog
    {
        public LogViewerHelpDialog()
        {
            var label = new Label("Log viewer help");
            label.Font = label.Font.WithSize(15).WithWeight(Xwt.Drawing.FontWeight.Bold);

            var markdown = new MarkdownView();
            using(var stream = typeof(LogViewerHelpDialog).Assembly.GetManifestResourceStream("Emul8.Extensions.AdvancedLoggerViewer.LogViewerHelpFile.txt"))
            {
                using(var reader = new StreamReader(stream))
                {
                    markdown.Markdown = reader.ReadToEnd();
                }
            }

            var box = new VBox();
            box.PackStart(label);
            box.PackStart(markdown, true);

            Content = box;
            Buttons.Add(new DialogButton(Command.Ok));

            Width = 1000;
            Height = 300;
        }
    }
}

