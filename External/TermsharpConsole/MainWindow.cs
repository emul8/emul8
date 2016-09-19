//
// Copyright (c) Antmicro
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//
using Xwt;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Emul8.Utilities;
using Xwt.Drawing;
using System.IO;
using TermSharp;
using TermSharp.Vt100;
using TermSharp.Rows;

namespace TermsharpConsole
{
	public class MainWindow : Window
	{
		public MainWindow(string title, string pty)
		{
			Title = title;
			Width = 700;
			Height = 400;

			terminal = new TermSharp.Terminal();
			Content = terminal;
			terminal.InnerMargin = new WidgetSpacing(5, 0, 5, 0);
			Padding = new WidgetSpacing();

			terminal.Cursor.Enabled = true;

			Font.RegisterFontFromFile(Path.Combine(Directory.GetCurrentDirectory(), "External/TermsharpConsole/RobotoMono-Regular.ttf"));
			var robotoMonoFont = Font.FromName("Roboto Mono");
			if(robotoMonoFont.Family.Contains("Roboto"))
			{
				terminal.CurrentFont = robotoMonoFont;
			}

			var contextMenu = new Menu();

			var copyMenuItem = new MenuItem("Copy");
			copyMenuItem.Clicked += (sender, e) => Clipboard.SetText(terminal.CollectClipboardData().Text);
			contextMenu.Items.Add(copyMenuItem);

			var pasteMenuItem = new MenuItem("Paste");
			contextMenu.Items.Add(pasteMenuItem);

			terminal.ContextMenu = contextMenu;

			CloseRequested += delegate
			{
				Application.Exit();
			};

			terminal.SetFocus();

			var readerThread = new Thread(() =>
			{
				var stream = new PtyUnixStream(pty);
				var vt100decoder = new TermSharp.Vt100.Decoder(terminal, stream.WriteByte, new ConsoleDecoderLogger());
				var utfDecoder = new ByteUtf8Decoder(vt100decoder.Feed);

				Application.Invoke(() =>
				{
					pasteMenuItem.Clicked += delegate
					{
						var text = Clipboard.GetText();
						var textAsBytes = Encoding.UTF8.GetBytes(text);
						foreach(var b in textAsBytes)
						{
							stream.WriteByte(b);
						}
					};
				});

				var encoder = new TermSharp.Vt100.Encoder(x => 
				{
					terminal.ClearSelection();
					terminal.MoveScrollbarToEnd();
					stream.WriteByte(x); 
				});

				terminal.KeyPressed += (s, a) =>
				{
					a.Handled = true;

					var modifiers = a.Modifiers;
					if(!Utilities.IsOnOsX)
					{
						modifiers &= ~(ModifierKeys.Command);
					}

					if(modifiers== ModifierKeys.Shift)
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

				var buffer = new List<byte>();
				var noTimeoutNextTime = true;
				while(true)
				{
					if(noTimeoutNextTime)
					{
						noTimeoutNextTime = false;
					}
					else
					{
						stream.ReadTimeout = 10;
					}
					var readByte = buffer.Count > 1024 ? BufferFull : stream.ReadByte();
					if(readByte == StreamClosed)
					{
						Application.Invoke(Application.Exit);
						return;
					}
					if(readByte >= 0)
					{
						buffer.Add((byte)readByte);
					}
					else
					{
						var bufferToWrite = buffer;
						Application.Invoke(() =>
						{
							foreach (var b in bufferToWrite)
							{
								utfDecoder.Feed(b);
							}
						});
						buffer = new List<byte>();
						noTimeoutNextTime = true;
					}
				}
			})
			{ IsBackground = true };

			readerThread.Start();
		}

		protected override void OnShown()
		{
			base.OnShown();
			double unused;
			var lineHeight = ((MonospaceTextRow)terminal.GetFirstScreenRow(out unused)).LineHeight;
			Height = 30 * lineHeight + Height - terminal.ScreenSize;
		}

		private readonly Terminal terminal;

		private const int BufferFull = -3;
		private const int StreamClosed = -1;
	}
}

