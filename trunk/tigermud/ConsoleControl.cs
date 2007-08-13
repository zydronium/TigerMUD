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

using System;

namespace TigerMUD
{
	/// <summary>
	/// Allows the MUD Engine to be controlled from the 
	/// console window.
	/// </summary>
	public class ConsoleControl
	{
		public ConsoleControl()
		{
		}

		/// <summary>
		/// Gets a line of user input from the console.
		/// </summary>
		/// <returns>A line of user input.</returns>
		public string GetUserInput()
		{
			// Write a friendly prompt
			Console.Write("tigermud: ");
			// Get input from the command line.
			return Console.ReadLine();
		}

		/// <summary>
		/// Processes a console command.
		/// </summary>
		/// <param name="command">The full command line.</param>
		public void HandleConsoleCommand(string command)
		{
			try 
			{
				switch(command.Split(' ')[0])
				{
					case "status":
						ShowServerStatus();
						break;
					case "users":
						ListUsersOnline();
						break;
					case "who":
						ListUsersOnline();
						break;
					case "":
						break;
					default:
						DisplayHelp();
						break;
				}
			} 
			catch (Exception ex)
			{
                Lib.PrintLine("Error: " + ex.Message + ex.StackTrace);
			}
		}

		#region Help
		/// <summary>
		/// Show a list of commands and what they do.
		/// </summary>
		public void DisplayHelp()
		{
			Console.WriteLine("TigerMUD Console Commands:");
			Console.WriteLine("\tshutdown\t\tclose the MUD server");
			Console.WriteLine("\trestart\t\t\trestart the MUD server");
			Console.WriteLine("\tstatus\t\t\tshow MUD server status");
			Console.WriteLine("\tusers\t\t\tshow a list of online users");
			Console.WriteLine("\twho\t\t\tshow a list of online users");

		}
		#endregion

		#region Commands
		/// <summary>
		/// Display server status in the console window.
		/// </summary>
		private void ShowServerStatus()
		{
			Console.WriteLine("MUD Status:");
      Console.WriteLine("Authenticated users: " + Lib.GetConnectedActorCount());
      Console.WriteLine("Open connections: " + Lib.connections);
    }

		/// <summary>
		/// List all the users that are online by their shortnames.
		/// </summary>
		private void ListUsersOnline()
		{
			Console.WriteLine("MUD Users:");
    
			for (int i=Lib.actors.Count-1;i>=0;i--)
			{
				Actor user=(Actor)Lib.actors[i];
				if (user["type"].ToString().Equals("user") && Lib.ConvertToBoolean(user["connected"])) Lib.PrintLine("\t" + user["shortname"] + " (" + user.UserIPAddress + ")");      
			}
            Lib.PrintLine("Authenticated users: " + Lib.GetConnectedActorCount());
            Lib.PrintLine("Open connections: " + Lib.connections);
		}
		#endregion
	}
}