using System;

namespace Tiger.Loader.Lib
{
	/// <summary>
	/// Summary description for SubAppBase.
	/// </summary>

	[Serializable]
	public abstract class SubAppBase : MarshalByRefObject, Tiger.Loader.Lib.ISubApp_V1
	{
		public event Tiger.Loader.Lib.MessageOutEventHandler MessageOut;

		#region Properties
		private	Version	version		= null;
		private	string	shortName	= @"";
		private string	helpText	= @"Help text!";
		private	bool	unloadable	= false;
		#endregion

		#region Constructor
		public SubAppBase(Version version, string shortName)
		{
			this.version	= version;
			this.shortName	= shortName;
		}
		//AJ: SubApp's that are in this assembly need to have unloadable set true
		internal SubAppBase(Version version, string shortName, bool unloadable)
		{
			this.version	= version;
			this.shortName	= shortName;
			this.unloadable	= true;
		}
		#endregion

		#region Interface Property Accessors

		#region Version
		public Version Version
		{
			get
			{
				return this.version;
			}
		}
		#endregion

		#region ShortName
		public string ShortName
		{
			get
			{
				return this.shortName;
			}
		}
		#endregion

		#region HelpText
		public string HelpText
		{
			get
			{
				return this.helpText;
			}
		}
		#endregion

		#region Unloadable
		public bool Unloadable
		{
			get
			{
				return this.unloadable;
			}
		}
		#endregion

		#endregion

		#region Abstract Interface Methods

		public abstract bool Start();
		public abstract bool Restart();
		public abstract bool Stop();
		public abstract void MessageIn(int fromID, string fromApp, int toID, string toApp, string messageText);

		#endregion

		protected void SetHelpText(string helpText)
		{
			this.helpText	= helpText;
		}

		protected virtual void FireMessageOut(int fromID, string fromApp, int toID, string toApp, string messageText)
		{
			this.MessageOut(fromID, fromApp, toID, toApp, messageText);
		}
	}
}
