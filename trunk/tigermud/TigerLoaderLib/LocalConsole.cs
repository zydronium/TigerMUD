using System;
using System.Collections;
using System.IO;

namespace Tiger.Loader.Lib
{
	[Serializable]
	public class LocalConsole : Tiger.Loader.Lib.SubAppBase
	{
		#region Properties
		private TextReader	consoleIn	= null;
		private TextWriter	consoleOut	= null;
		private	Loader		loader		= null;

		private bool		exitLoader	= false;
		#endregion

		#region Exitloader
		public bool ExitLoader
		{
			get
			{
				return this.exitLoader;
			}
		}
		#endregion

		public LocalConsole(TextReader consoleIn, TextWriter consoleOut, Loader loader) : base(new Version(0, 1), @"LC", true)
		{
			this.consoleIn	= consoleIn;
			this.consoleOut	= consoleOut;
			this.loader		= loader;

			this.loader.LoadLocal(this);
			this.consoleOut.WriteLine(@"Connected 'Local Console' to 'Loader'");
		}

		public override bool Start()
		{
			return true;
		}

        public override bool Stop()
		{
			return true;
		}

        public override bool Restart()
		{
			return true;
		}

		#region MessageIn
		public override void MessageIn(int fromID, string fromApp, int toID, string toApp, string messageText)
		{
			Console.WriteLine(messageText);
		}
		#endregion
	}
}