//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using System;
using Xwt;
using Emul8.Peripherals.UART;
using AntShell.Terminal;
using Emul8.Utilities;
using TermSharp;
using System.Threading;
using TermSharp.Vt100;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Emul8.Plugins.XwtProviderPlugin;

namespace Emul8.CLI
{
    public class TerminalWidget : Widget
    {
        public TerminalWidget()
        {
            terminal = new Terminal();
            terminalInputOutputSource = new TerminalIOSource(terminal);
            IO = new DetachableIO(terminalInputOutputSource);
            IO.BeforeWrite += b =>
            {
                // we do not check if previous byte was '\r', because it should not cause any problem to 
                // send it twice
                if(ModifyLineEndings && b == '\n')
                {
                    IO.Write((byte)'\r');
                }
            };

            terminal.InnerMargin = new WidgetSpacing(5, 5, 5, 5);
            terminal.Cursor.Enabled = true;
            terminal.ContextMenu = CreatePopupMenu();

            var fontFile = typeof(TerminalWidget).Assembly.FromResourceToTemporaryFile("RobotoMono-Regular.ttf");
            Xwt.Drawing.Font.RegisterFontFromFile(fontFile);
            // here we try to load the robot font; unfortunately it is loaded even if there is
            // no such font available; because of that we have to check whether it is in fact
            // the font wanted
            var robotoFont = Xwt.Drawing.Font.FromName("Roboto Mono").WithSize(10);
            if(robotoFont.Family.Contains("Roboto Mono"))
            {
                terminal.CurrentFont = robotoFont;
            }

            var encoder = new TermSharp.Vt100.Encoder(x =>
            {
                terminal.ClearSelection();
                terminal.MoveScrollbarToEnd();
                terminalInputOutputSource.HandleInput(x);
            });

            terminal.KeyPressed += (s, a) =>
            {
                a.Handled = true;

                var modifiers = a.Modifiers;
                if(!Misc.IsOnOsX)
                {
                    modifiers &= ~(ModifierKeys.Command);
                }

                if(modifiers == ModifierKeys.Shift)
                {
                    if(a.Key == Key.PageUp)
                    {
                        terminal.PageUp();
                        return;
                    }
                    if(a.Key == Key.PageDown)
                    {
                        terminal.PageDown();
                        return;
                    }
                }
                encoder.Feed(a.Key, modifiers);
            };
            Content = terminal;
        }

        public TerminalWidget(UARTBackend backend) : this()
        {
            backend.BindAnalyzer(IO);
        }

        public TerminalWidget(UARTBackend backend, Func<TerminalWidget, MenuItem[]> menuItemProvider) : this(backend)
        {
            additionlMenuItemProvider = menuItemProvider;
            terminal.ContextMenu = CreatePopupMenu();
        }

        public void Clear()
        {
            terminal.Clear();
        }

        public bool ModifyLineEndings
        { 
            get { return modifyLineEndings; }
            set
            { 
                modifyLineEndings = value; 
                terminal.ContextMenu = CreatePopupMenu(); 
            }
        }

        public DetachableIO IO { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(IO != null)
            {
                IO.Dispose();
                IO = null;
            }
        }

        private Menu CreatePopupMenu()
        {
            var popup = new Menu();

            var copyItem = new MenuItem("Copy");
            copyItem.Clicked += delegate
            {
                Clipboard.SetText(terminal.CollectClipboardData().Text);
            };
            popup.Items.Add(copyItem);

            var pasteItem = new MenuItem("Paste");
            pasteItem.Clicked += delegate
            {
                var text = Clipboard.GetText();
                if(string.IsNullOrEmpty(text))
                {
                    return;
                }
                var textAsBytes = Encoding.UTF8.GetBytes(text);
                foreach(var b in textAsBytes)
                {
                    terminalInputOutputSource.HandleInput(b);
                }
            };
            popup.Items.Add(pasteItem);

            if(additionlMenuItemProvider != null)
            {
                foreach(var item in additionlMenuItemProvider(this))
                {
                    popup.Items.Add(item);
                }
            }

            return popup;
        }

        private readonly Func<TerminalWidget, MenuItem[]> additionlMenuItemProvider;
        private readonly Terminal terminal;
        private readonly TerminalIOSource terminalInputOutputSource;
        private bool modifyLineEndings;
    }
}

