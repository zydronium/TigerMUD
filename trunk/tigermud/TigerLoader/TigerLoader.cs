using System;
using System.Collections;
using System.ServiceProcess;

namespace Tiger.Loader
{
	public enum AppMode
	{
		UNSET	= 0,
		Console	= 1,
		Service	= 2
	}
	class TigerLoader
	{
		[STAThread]
		static void Main(string[] args)
		{
			//AJ: **TESTING** change this to switch between console and service mode
			AppMode	appMode = AppMode.Console;
			//AJ: **TESTING**

			if(appMode == AppMode.Console)
			{
				//AJ: **TESTING**
				ArrayList	subApps	= new ArrayList();
				string[]	subApp	= new string[3];

				//AJ: Directory
				//Adam's Localtion
				//subApp[0] = @"C:\TigerMUD\TigerRemoteConsole\bin\Debug";
                subApp[0] = TigerMUD.Lib.PathtoRoot;
                
                
				//Andy'sLocation
				//subApp[0] = @"J:\Code\CVS\TigerMUD\TigerMUD\TigerRemoteConsole\bin\Debug";
				//AJ: Filename
				subApp[1] = @"TigerRemoteConsole.dll";
				//AJ: Object Name
				subApp[2] = @"Tiger.RemoteConsole.Server";

				subApps.Add(subApp);
				//AJ: **TESTING**

				Console.WriteLine(@"Starting 'Loader'");

				//AJ: Start the loader running
				Tiger.Loader.Lib.Loader l = new Tiger.Loader.Lib.Loader(subApps);

				Console.WriteLine(@"Started 'Loader'");

				Console.WriteLine(@"Connecting 'Local Console' to 'Loader'");
				//AJ: Do Local Console
				Tiger.Loader.Lib.LocalConsole lc = new Tiger.Loader.Lib.LocalConsole(Console.In, Console.Out, l);

				Console.ReadLine();

				while(!lc.ExitLoader)
				{
					//AJ: hang around until we want to quit
					System.Threading.Thread.Sleep(500);
				}

				l.UnloadAll();
			}
			else if(appMode == AppMode.Service)
			{
				//AJ: Do Service
				ServiceBase[] ServicesToRun = new ServiceBase[] {new Service()};
				System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			}
			else
			{
				throw new Exception(@"Application Mode not specifed");
			}
		}
	}
}
