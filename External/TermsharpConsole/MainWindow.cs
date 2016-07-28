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
using Terminal.Vt100;
using Emul8.Utilities;
using Terminal.Rows;

namespace TermsharpConsole
{
	public class MainWindow : Window
	{
		public MainWindow(string title, string pty)
		{
			Title = title;
			Width = 700;
			Height = 400;

			terminal = new Terminal.Terminal();
			Content = terminal;
			terminal.InnerMargin = new WidgetSpacing(5, 5, 5, 5);
			Padding = new WidgetSpacing();

			terminal.Cursor.Enabled = true;

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
				while(true)
				{
					var stream = new PtyUnixStream(pty);
					var vt100decoder = new Terminal.Vt100.Decoder(terminal, stream.WriteByte, new ConsoleDecoderLogger());
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

					var encoder = new Terminal.Vt100.Encoder(x => 
					{ 
						terminal.MoveScrollbarToEnd();
						stream.WriteByte(x); 
					});

					terminal.KeyPressed += (s, a) =>
					{
						a.Handled = true;
						encoder.Feed(a.Key, a.Modifiers);
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
						var readByte = buffer.Count > 1024 ? -3 : stream.ReadByte();
						if(readByte == -1)
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
						if (readByte == -1)
						{
							Application.Invoke(Application.Exit);
							return;
						}
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

		private readonly Terminal.Terminal terminal;
	}
}

