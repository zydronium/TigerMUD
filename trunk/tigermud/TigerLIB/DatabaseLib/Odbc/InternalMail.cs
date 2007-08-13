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
using TigerMUD.DatabaseLib;

namespace TigerMUD.DatabaseLib.Odbc
{
	/// <summary>
	/// ODBC Implementation of IInternalMail
	/// </summary>
	public class InternalMail : BaseOdbc, IInternalMail
	{
		public InternalMail(string connectionString) : base(connectionString)
		{
		}

		public DataTable GetAllMessages(string shortName)
		{
			string statement = "SELECT * FROM mudmail WHERE M_Receiver = '" + shortName + "';";
			return ExecuteDataTable(statement);
		}

		public DataTable GetAllUnreadMessages(string shortName)
		{
			string statement = "SELECT * FROM mudmail WHERE M_Receiver = '" + shortName + "' AND M_Read = false;";
			return ExecuteDataTable(statement);
		}

		public DataTable GetAllSentMessages(string shortName)
		{
			string statement = "SELECT * FROM mudmail WHERE M_Sender = '" + shortName + "' AND M_Read = false;";
			return ExecuteDataTable(statement);
		}
        
		public void MarkAsRead(int mailID)
		{
			string statement = "UPDATE mudmail SET M_Read=true WHERE M_ID = " + mailID + ";";
			ExecuteDataTable(statement);
		}

		public bool SendMessage(string M_Sender, string M_Receiver, string M_Subject, string M_Body)
		{
			string statement = "INSERT INTO mudmail (M_Sender,M_Receiver, M_Subject,M_Body,M_Read,M_Sender_Delete,M_Receiver_Delete,M_Created) VALUES ('" 
				+ M_Sender + "','" + M_Receiver + "','" + M_Subject + "','" + M_Body + "'," + Convert.ToByte(false) + "," + Convert.ToByte(false) + "," + Convert.ToByte(false) + ",'" + DateTime.Now + "');";
			
			try
			{
				ExecuteDataTable(statement);
				return true;
			}
			catch (Exception ex)
			{
				Lib.PrintLine("EXCEPTION sending mail: " + ex.Message + ex.StackTrace);
				Lib.PrintLine("SQL statement was: " + statement);

				return false;
			}

		}

	}
}
