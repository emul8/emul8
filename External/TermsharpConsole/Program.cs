using System;
using System.Threading;
using Xwt;
using Xwt.Drawing;

namespace TermsharpConsole
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if(args.Length != 2)
			{
				Console.Error.WriteLine("Usage: title pty");
				return;	
			}

			var title = args[0];
			var pty = args[1];

			Application.Initialize(ToolkitType.Gtk);
			var window = new MainWindow(title, pty);
			window.Show();

			Application.Run();

			window.Dispose();
			Application.Dispose();
		}
	}
}
