using System;

namespace Tiger.Loader.Lib
{
	[Serializable]
	public class SubAppContainer
	{
		#region Properties
		private AppDomain		appDomain		= null;
		private AppDomainSetup	appDomainSetup	= new AppDomainSetup();
		private RemoteLoader	remoteLoader	= null;
		private ISubApp_V1		app				= null;
		private	string			directory		= @"";
		private	string			filename		= @"";
		private	string			objectName		= @"";
		private	bool			localSubApp		= false;
		#endregion

		#region Property Accessors

		#region AppDomain
		public AppDomain AppDomain
		{
			get
			{
				return this.appDomain;
			}
			set
			{
				this.appDomain	= value;
			}
		}
		#endregion

		#region App
		public ISubApp_V1 App
		{
			get
			{
				return this.app;
			}
			set
			{
				this.app	= value;
			}
		}
		#endregion

		#region Directory
		public string Directory
		{
			get
			{
				return this.directory;
			}
		}
		#endregion

		#region Filename
		public string Filename
		{
			get
			{
				return this.filename;
			}
		}
		#endregion

		#region ObjectName
		public string ObjectName
		{
			get
			{
				return this.objectName;
			}
		}
		#endregion

		#endregion

		#region Constructors
		public SubAppContainer(string directory, string filename, string objectName)
		{
			this.directory	= directory;
			this.filename	= filename;
			this.objectName	= objectName;
		}
		public SubAppContainer(string directory, string filename, string objectName, bool autoLoad)
		{
			this.directory	= directory;
			this.filename	= filename;
			this.objectName	= objectName;

			if(autoLoad)
				this.Load();
		}
		internal SubAppContainer(string directory, string filename, string objectName, ISubApp_V1 localSubApp)
		{
			this.directory		= directory;
			this.filename		= filename;
			this.objectName		= objectName;
			this.appDomain		= System.AppDomain.CurrentDomain;
			this.localSubApp	= true;
			this.app			= localSubApp;
		}
		#endregion

		#region Methods

		#region Load
		public void Load()
		{
			if(app == null && !this.localSubApp)
			{
				//AJ: The base directory is the same as this appdomain because we'll need the ISubApp_V1
				this.appDomainSetup.ApplicationBase	= System.AppDomain.CurrentDomain.BaseDirectory;
				this.appDomainSetup.ApplicationName	= this.objectName;
				this.appDomainSetup.ShadowCopyFiles	= @"true";

				//AJ: Create the new AppDomain
				this.appDomain						= AppDomain.CreateDomain(this.objectName, null, this.appDomainSetup);

				//AJ: Create the Remote Loader in the new AppDomain and retrive a proxy copy of it
				this.remoteLoader					= (RemoteLoader)  appDomain.CreateInstanceFromAndUnwrap("TigerLoaderLib.dll", "Tiger.Loader.Lib.RemoteLoader");

				//AJ: Use the Remote Loader to load the SubApp and return a proxy copy of it
				this.app							= (ISubApp_V1) this.remoteLoader.LoadObject(this.directory, this.filename, this.objectName);
				this.app.Start();
			}
		}
		#endregion

		#region Unload
		public void Unload()
		{
			if(app != null && !this.localSubApp)
			{
				//AJ: Stop the SubApp gracefully
				this.app.Stop();
				this.app	= null;

				//AJ: Remove the Remote Loader
				this.remoteLoader	= null;

				//AJ: Unload the AppDomain
				AppDomain.Unload(this.appDomain);
				this.appDomain = null;
			}
		}
		#endregion

		#region Reload
		public void Reload()
		{
			this.Unload();
			this.Load();
		}
		#endregion

		#endregion
	}
}