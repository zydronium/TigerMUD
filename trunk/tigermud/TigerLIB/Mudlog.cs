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
using System.Data;
using System.Net.Sockets;
using System.Net;
using TigerMUD.CommsLib;

namespace TigerMUD
{
	// This class defines a mud system log where events are recorded with the Add method
	public class Mudlog
	{
		public void Add(IUserSocket socket, string username, string logmessage)
		{
			try
			{
				string endPointId = "unknown";

				if(socket != null)
				{
					endPointId = socket.ClientEndpointId;
				}
				Lib.dbService.MudLog.Log(logmessage,
					endPointId,
					username);
				Lib.PrintLine(DateTime.Now.ToString() + " - " + logmessage);
			}
			catch (Exception ex)
			{
				Lib.log.Add("mudlog.add", "Exception. " + ex.Message + ex.StackTrace);
			}
		}

		public void Add(Actor actor, string logmessage)
		{
			string clientip="no clientip";
			if(actor.UserSocket.Connected) 
			{
				clientip = actor.UserSocket.ClientEndpointId;
			}
			
			try 
			{
                Lib.dbService.MudLog.Log(logmessage, clientip, actor["shortname"].ToString());
				Lib.PrintLine(DateTime.Now.ToString() + " - " + logmessage);
			}
			catch (Exception ex ) 
			{
				Lib.PrintLine();
				Lib.PrintLine();
				Lib.PrintLine(DateTime.Now.ToString() + " - " + ex.Message + ex.StackTrace);
				Lib.PrintLine();
				Lib.PrintLine("THE MUD SERVER HAS CRASHED In Mudlog.cs in Add Log. Note the errors above for a clue to what failed and press ENTER to exit.");
			}
		}

		public void Add(string procedurename, string logmessage)
		{
			try 
			{
				Lib.dbService.MudLog.Log(logmessage, "No clientip", procedurename);
				Lib.PrintLine(procedurename + "-" + logmessage);
			}
			catch (Exception ex ) 
			{
				Lib.PrintLine();
				Lib.PrintLine();
				Lib.PrintLine(DateTime.Now.ToString() + " - " + ex.Message + ex.StackTrace);
				Lib.PrintLine();
				Lib.PrintLine("THE MUD SERVER HAS CRASHED In Mudlog.cs in Add Log. Note the errors above for a clue to what failed and press ENTER to exit.");
			}
		}
   
		public string Read()
		{
			string wholelog = "";
		
			try 
			{
				DataTable logTable = Lib.dbService.MudLog.GetAllEntries();
				wholelog = Lib.DumpTableToString(logTable, false, ",");
			}
			catch (Exception ex ) 
			{
				Lib.log.Add("mudlog.read", "EXCEPTION " + ex.Message + ex.StackTrace);
			}
			return wholelog;
		}
    
		public string Delete()
		{
			try 
			{
				Lib.dbService.MudLog.ClearLog();
			} 
			catch (Exception ex ) 
			{
				Lib.log.Add("mudlog.delete", "EXCEPTION " + ex.Message + ex.StackTrace);
			}
			return "Log deleted.\r\n";
		}
	}
}
