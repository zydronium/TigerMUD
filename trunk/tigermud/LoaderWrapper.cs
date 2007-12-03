using System;

namespace TigerMUD
{
	[Serializable]
	public class LoaderWrapper : Tiger.Loader.Lib.SubAppBase
	{
		#region Properties
		private Threadmanager threadManager = new Threadmanager();
		#endregion

		#region Constructors
		public LoaderWrapper() : base(new Version(Lib.Serverversion), @"MUD")
		{
		}
		#endregion

		#region Start
		public override bool Start()
		{
			// This section of code finds and loads the tigermud.xml file.
			// The code is more complex because it supports the location of tigermud.xml in both 
			// the dev team environment and normal user runtime environments.
			string path = AppDomain.CurrentDomain.BaseDirectory + @"\TigerMUD.xml";

			try
			{
				Lib.Serverinfo = Lib.Readxmldoc(path);
                threadManager.Start(false);
			}
			catch (Exception ex)
			{
				Lib.PrintLine(ex.Message + ex.StackTrace);
				//throw new Exception("Cannot find TigerMUD.xml configuration file. Server load failed.");
			}

			return true;
		}
		#endregion

		#region Restart
        public override bool Restart()
		{
			this.Stop();
			this.Start();
			return true;
		}
		#endregion

		#region Stop
        public override bool Stop()
		{
			threadManager.Stop();
			return true;
		}
		#endregion

		#region MessageIn
		public override void MessageIn(int fromID, string fromApp, int toID, string toApp, string messageText)
		{
		}
		#endregion
	}
}