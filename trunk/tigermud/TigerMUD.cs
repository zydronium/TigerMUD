#region TigerMUD License
/*
/-------------------------------------------------------------\
|    _______  _                     __  __  _    _  _____     |
|   |__   __|(_)                   |  \/  || |  | ||  __ \    |
|      | |    _   __ _   ___  _ __ | \  / || |  | || |  | |   |
|      | |   | | / _` | / _ \| '__|| |\/| || |  | || |  | |   |
|      | |   | || (_| ||  __/| |   | |  | || |__| || |__| |   |
|      |_|   |_| \__, | \___||_|   |_|  |_| \____/ |_____/    |
|                 __/ |                                       |
|                |___/                  Copyright (c) 2004    |
\-------------------------------------------------------------/

TigerMUD. A Multi User Dungeon engine.
Copyright (C) 2004 Adam Miller et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

You can contact the TigerMUD developers at www.tigermud.com or at
http://sourceforge.net/projects/tigermud.

The full licence can be found in <root>/docs/TigerMUD_license.txt
*/
#endregion

//using System;
using System.ServiceProcess;
//using System.IO;

using System;
using System.Collections;
using System.Data;
using System.Data.Odbc;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;

namespace TigerMUD
{
    public class Dummy
    { 
        //remove for release
        static void Main(string[] args)
        {
            Server myServer = new Server();
            myServer.Start();
        }
        //end remove
    }


    [Serializable]
    public class Server : MarshalByRefObject
	{
        Threadmanager threadManager = null;
        public bool exit = false;
        

        public bool Started
        {
            get { return Lib.ServerStarted; }
            set { Lib.ServerStarted = value; }
        }
	 
        public void Stop()
        {
            threadManager.Stop();
            // logwriters are opened in Start() method and closed in Stop() method
            Lib.serverlogwriter.Close();
            Lib.commandlogwriter.Close();
            Started = false;
            return;

        }
        public void Start()
        {
            //Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Lib.PathtoRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            Lib.PathtoRootAssemblies = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"bin" + Lib.PathtoDebugorRelease));
            Lib.PathtoRootRemoteConsole = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"tigerremoteconsole"));
            Lib.PathtoRootRemoteConsoleAssemblies = Path.GetFullPath(Path.Combine(Lib.PathtoRootRemoteConsole, @"..\bin\debug" + Lib.PathtoDebugorRelease));
            Lib.PathtoRootScriptsandPlugins = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"tigermudscriptsandplugins"));
            Lib.PathtoRootScriptsandPluginsAssemblies = Path.GetFullPath(Path.Combine(Lib.PathtoRootScriptsandPlugins, @"bin\Debug" + Lib.PathtoDebugorRelease));
            Lib.PathtoRootTigerLoaderLib = Path.GetFullPath(Path.Combine(Lib.PathtoRoot, @"tigerloaderlib"));
            Lib.PathtoRootTigerLoaderLibAssemblies = Path.GetFullPath(Path.Combine(Lib.PathtoRootTigerLoaderLib, @"bin\" + Lib.PathtoDebugorRelease));
            
            //Lib.PathtoRootRemoteConsoleAssemblies = Path.GetFullPath(Environment.CurrentDirectory);
            //Lib.PathtoRootRemoteConsole = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\.."));
            
            // this must come before ANY printing to console
            // logwriters are opened in Start() method and closed in Stop() method
            Lib.serverlogwriter = new StreamWriter(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.serverLogFileName)), true);
            Lib.serverlogwriter.AutoFlush = true;
            Lib.commandlogwriter = new StreamWriter(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, Lib.commandLogFileName)), true);
            Lib.commandlogwriter.AutoFlush = true;
            //Lib.PrintLine("Environment.CurrentDirectory=" + Environment.CurrentDirectory);
            
            //Lib.PrintLine("AppDomain.CurrentDomain.BaseDirectory=" + AppDomain.CurrentDomain.BaseDirectory);
            //Lib.PrintLine("PathtoRoot=" + Lib.PathtoRoot);
            //Lib.PrintLine("PathtoRootAssemblies=" + Lib.PathtoRootAssemblies);
            //Lib.PrintLine("PathtoRootRemoteConsole=" + Lib.PathtoRootRemoteConsole);
            //Lib.PrintLine("PathtoRootRemoteConsoleAssemblies=" + Lib.PathtoRootRemoteConsoleAssemblies);
            //Lib.PrintLine("PathtoRootScriptsandPlugins=" + Lib.PathtoRootScriptsandPlugins);
            //Lib.PrintLine("PathtoRootScriptsandPluginsAssemblies=" + Lib.PathtoRootScriptsandPluginsAssemblies);
            //Lib.PrintLine("PathtoRootTigerLoaderLib=" + Lib.PathtoRootTigerLoaderLib);
            //Lib.PrintLine("PathtoRootTigerLoaderLibAssemblies=" + Lib.PathtoRootTigerLoaderLibAssemblies);
            
           

            //bool exit = false;

			// This section of code finds and loads the tigermud.xml file.
			// The code is more complex because it supports the location of tigermud.xml in both 
			// the dev team environment and normal user runtime environments.

			try
			{
                Lib.Serverinfo = Lib.Readxmldoc(Path.GetFullPath(Path.Combine(Lib.PathtoRoot, "tigermud.xml")));
			}
			catch (Exception ex)
			{
				Lib.PrintLine("EXCEPTION in Server.Start (reading tigermud.xml): " + ex.Message + ex.StackTrace);
			}

			// Only run in console mode if explicitly told
			// to do so by tigermud.xml.
			if(Lib.Serverinfo.ServerMode == "console")
			{
				threadManager = new Threadmanager();
                try
                {
                    threadManager.Start(false);
                }
                catch (Exception ex)
                {
                    Lib.PrintLine("*********************** SERVER CRASH ***************************\r\n\r\n" + ex.Message + ex.StackTrace);
                    Lib.log.Add("Error on MUD Startup:",ex.Message + ex.StackTrace);
                    Console.ReadLine();
                    return;
                }


				// Listen for user input
				Lib.PrintLine("\n\nTigerMUD Console. Type 'help' to see a list of commands.");
				
                ConsoleControl control = new ConsoleControl();

				// Loop on user commands
                Started = true;
				while(!exit)
				{
					string command = control.GetUserInput().ToLower();

					switch(command)
					{
						case "shutdown":
							foreach (Actor actor in Lib.actors)
							{
								if (Lib.ConvertToBoolean(actor["connected"])) actor.SendAnnouncement("\r\nServer shutdown initiated from console. Server coming down NOW.\r\n");
							}

							threadManager.Stop();
							exit = true;
							break;
						case "restart":
							threadManager.Stop();
							threadManager.Start(false);
							break;
						default:
							control.HandleConsoleCommand(command);
							break;
					}
				}
			}
			else
			{
				// We are running in service mode
				ServiceBase[] ServicesToRun = new ServiceBase[] {new TigerMUDService()};
				System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			}			
		}
	}
}