using System;
using System.IO;

namespace TermsharpConsole
{
	public static class Utilities
	{
		public static bool IsOnOsX
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.MacOSX)
				{
					return true;
				}
				return Directory.Exists("/Library") && Directory.Exists("/Applications");
			}
		}
	}
}

