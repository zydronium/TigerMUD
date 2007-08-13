using System;
using System.Collections;

namespace Tiger.Loader.Lib
{
	[Serializable]
	public class Loader
	{
		#region Properties
		private Hashtable	LoadedApps			= new Hashtable();
		#endregion

		#region Constructor
		public Loader()
		{
		}
		public Loader(ArrayList subApps)
		{
			foreach(string[] subApp in subApps)
			{
				this.Load(subApp[0], subApp[1], subApp[2]);
			}
		}
		#endregion

		#region Methods

		#region Load
		private bool Load(string directory, string filename, string objectName)
		{
			try
			{
				Console.WriteLine(@"Loading '" + objectName + "'");
				Console.WriteLine(@"From '" + directory + @"\" + filename + "'");
				//AJ: create the sub app container with the assembly details
				SubAppContainer sac = new SubAppContainer(directory, filename, objectName, true);
				this.LoadedApps.Add(sac.App.ShortName, sac);
				sac.App.MessageOut+=new MessageOutEventHandler(App_MessageOut);
				Console.WriteLine(@"Loaded '" + objectName + "'");
				return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(@"Loading '" + objectName + "' Failed");
				Console.WriteLine(ex.Message + ex.StackTrace);
				return false;
			}
		}
		#endregion

		#region LoadLocal
		internal bool LoadLocal(ISubApp_V1 subApp)
		{
			System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetCallingAssembly().Location);

			SubAppContainer sac = new SubAppContainer(fi.DirectoryName, fi.Name + @"." + fi.Extension, subApp.GetType().FullName, subApp);

			if(!this.LoadedApps.ContainsKey(sac.App.ShortName))
			{
				this.LoadedApps.Add(sac.App.ShortName, sac);
				sac.App.MessageOut+=new MessageOutEventHandler(App_MessageOut);
			}

			return true;
		}
		#endregion

		#region Unload
		private bool Unload(string shortName)
		{
			try
			{
				if(this.LoadedApps.ContainsKey(shortName))
				{
					SubAppContainer sac = (SubAppContainer)this.LoadedApps[shortName];

					if(sac.App.Unloadable)
					{
						this.LoadedApps.Remove(shortName);
						sac.Unload();
						sac = null;
					}

					return true;
				}
				else
					return false;
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region UnloadAll
		public bool UnloadAll()
		{
			foreach(SubAppContainer sac in this.LoadedApps.Values)
			{
				string shortname = sac.App.ShortName;
				try
				{
					Console.WriteLine(@"Unloading '" + shortname + "'");
					sac.Unload();
					Console.WriteLine(@"Unloaded '" + shortname + "'");
				}
				catch(Exception ex)
				{
					Console.WriteLine(@"Unloading '" + shortname + "' Failed");
					Console.WriteLine(ex.Message + ex.StackTrace);
				}
			}

			return true;
		}
		#endregion

		#region Reload
		private bool Reload(string shortName)
		{
			try
			{
				if(this.LoadedApps.ContainsKey(shortName))
				{
					SubAppContainer sac = (SubAppContainer)this.LoadedApps[shortName];
					sac.Unload();
					sac.Load();
					return true;
				}
				else
				{
					if(this.LoadedApps.ContainsKey(shortName))
						this.LoadedApps.Remove(shortName);

					return false;
				}
			}
			catch
			{
				return false;
			}
		}
		#endregion

		#region Events

		public void App_MessageOut(int fromID, string fromApp, int toID, string toApp, string messageText)
		{
			//messageText = messageText.Replace("\r", @"");

			if(toApp.ToLower() == "ldr")
			{
				this.ProcessLoaderCommands(fromID, fromApp, toID, toApp, messageText);
			}
			else if(this.LoadedApps.ContainsKey(toApp))
			{
				//load destination app
				SubAppContainer sac = (SubAppContainer)this.LoadedApps[toApp];
				sac.App.MessageIn(fromID, fromApp, toID, toApp, messageText);
			}
			else
			{
				//AJ: do error - to app doesn't exist
				this.ReplyToSender(fromID, fromApp, @"Destination app doesn't exist");
			}
		}
		#endregion

		#region Loader Command Methods

		#region ProcessLoaderCommands
		private void ProcessLoaderCommands(int fromID, string fromApp, int toID, string toApp, string messageText)
		{
			string[] messageParts = messageText.Split(' ');

			switch (messageParts[0])
			{
					#region load
				case @"load":
					if(messageParts.Length == 4)
					{
						//AJ: load child app
						this.Load(messageParts[1], messageParts[2], messageParts[3]);
					}
					else
					{
						this.ReplyToSender(fromID, fromApp, @"Incorrect load arguments: " + messageText + "\r\n");
					}
					break;
					#endregion

					#region reload
				case @"reload":
					if(messageParts.Length == 2)
					{
						//AJ: unload child app
						this.Reload(messageParts[1]);
					}
					else
					{
                        this.ReplyToSender(fromID, fromApp, @"Incorrect reload arguments: " + messageText + "\r\n");
					}
					break;
					#endregion

					#region unload
				case @"unload":
					if(messageParts.Length == 2)
					{
						//AJ: unload child app
						this.Unload(messageParts[1]);
					}
					else
					{
                        this.ReplyToSender(fromID, fromApp, @"Incorrect unload arguments: " + messageText + "\r\n");
					}
					break;
					#endregion

					#region show
				case @"show":
					if(messageParts.Length == 2)
					{
						switch (messageParts[1])
						{
								#region loaded_apps
							case @"loaded_apps":
								this.ReplyToSender(fromID, fromApp, this.ShowLoadedApps());
								break;
								#endregion
						}
					}
					else
					{
                        this.ReplyToSender(fromID, fromApp, @"Incorrect show arguments: " + messageText + "\r\n");
					}
					break;
					#endregion

					#region help
				case @"help":
					if(messageParts.Length == 2)
					{
						if(this.LoadedApps.ContainsKey(messageParts[1]))
						{
							SubAppContainer sac = (SubAppContainer)this.LoadedApps[messageParts[1]];

							this.ReplyToSender(fromID, fromApp, sac.App.HelpText);
						}
						else
						{
                            this.ReplyToSender(fromID, fromApp, @"App doesn't exist: " + messageText + "\r\n");
						}
					}
					else
					{
						this.ReplyToSender(fromID, fromApp, @"Incorrect help arguments: " + messageText + "\r\n");
					}
					break;
					#endregion

					#region default
				default:
					this.ReplyToSender(fromID, fromApp, @"Unknown loader command: " + messageText + "\r\n");
					break;
					#endregion
			}
		}
		#endregion

		#region ShowLoadedApps
		private string ShowLoadedApps()
		{
			System.Text.StringBuilder output = new System.Text.StringBuilder();

			output.Append(System.Environment.NewLine);
			output.Append(@"::Loaded Apps::" + System.Environment.NewLine);
			output.Append(System.Environment.NewLine);

			foreach(SubAppContainer sac in this.LoadedApps.Values)
			{
				output.Append(@"Short Name: " + sac.App.ShortName + System.Environment.NewLine);
				output.Append(@"Version: " + sac.App.Version + System.Environment.NewLine);
				output.Append(@"AppDomain: " + sac.AppDomain.FriendlyName + System.Environment.NewLine);
				output.Append(@"Directory: " + sac.Directory + System.Environment.NewLine);
				output.Append(@"Filename: " + sac.Filename + System.Environment.NewLine);
				output.Append(@"Object Name: " + sac.ObjectName + System.Environment.NewLine);
				output.Append(@"Help: " + System.Environment.NewLine);
				output.Append(sac.App.HelpText + System.Environment.NewLine);
				output.Append(System.Environment.NewLine);
			}

			output.Append(System.Environment.NewLine);

			return output.ToString();
		}
		#endregion

		#endregion

		#region Utility Methods

		#region WriteToRemoteConsole
		private void WriteToRemoteConsole(string messageText)
		{
			//AJ: for deving
			Console.WriteLine(messageText + System.Environment.NewLine);
			//AJ: for deving

			if(this.LoadedApps.ContainsKey(@"RC"))
			{
				SubAppContainer sac = (SubAppContainer)this.LoadedApps[@"RC"];
				sac.App.MessageIn(0, @"LDR", 0, @"RC", messageText + System.Environment.NewLine);
			}
		}
		#endregion

		#region ReplyToSender
		private void ReplyToSender(int fromID, string fromApp, string messageText)
		{
		//AJ: return to sender
			SubAppContainer sac = (SubAppContainer)this.LoadedApps[fromApp];
			sac.App.MessageIn(0, @"LDR", fromID, fromApp, @"Unknown loader command: " + messageText);
		}
		#endregion

		#endregion

		#endregion
	}
}