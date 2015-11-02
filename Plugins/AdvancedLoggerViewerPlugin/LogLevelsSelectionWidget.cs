//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using System.Collections.Generic;
using Emul8.Logging;
using Xwt.Drawing;
using Xwt;

namespace Emul8.Plugins.AdvancedLoggerViewer
{
    public class LogLevelsSelectionWidget : HBox
    {
        private event Action<LogLevel, bool> selectionChanged;
        public event Action<LogLevel, bool> SelectionChanged
        {
            add
            {
                selectionChanged += value;
                foreach(var button in buttons)
                {
                    var level = LogLevel.Parse(button.Label);
                    value(level, button.Active);
                }
            }

            remove 
            {
                selectionChanged -= value;
            }
        }

        public LogLevelsSelectionWidget()
        {
            buttons = new List<ToggleButton>();
            var buttonsDictionary = new Dictionary<LogLevel, Image>() {
                { LogLevel.Noisy,    null },
                { LogLevel.Debug,    null },
                { LogLevel.Info,     StockIcons.Information.WithSize(IconSize.Small) },
                { LogLevel.Warning,  StockIcons.Warning.WithSize(IconSize.Small) },
                { LogLevel.Error,    StockIcons.Error.WithSize(IconSize.Small) },
            };

            foreach(var button in buttonsDictionary)
            {
                var b = new ToggleButton(button.Key.ToStringCamelCase()) { WidthRequest = 85 };
                if(button.Value != null)
                {
                    b.ImagePosition = ContentPosition.Left;
                    b.Image = button.Value;
                    b.Active = true;
                }

                b.Clicked += (sender, e) =>
                {
                    var tb = sender as ToggleButton;
                    var level = LogLevel.Parse(tb.Label);

                    var sc = selectionChanged;
                    if (sc != null)
                    {
                        sc(level, tb.Active);
                    }
                };

                PackStart(b);
                buttons.Add(b);
            }
        }

        private readonly List<ToggleButton> buttons;
    }
}

