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
	/// An ODBC implementation of ILibrary.
	/// </summary>
	public class Library : BaseOdbc, ILibrary
	{
		public Library(string connectionString) : base(connectionString)
		{
		}

		public DataTable GetAllSpells()
		{
			string statement = "SELECT * FROM mudspellstate where name='id';";
			return ExecuteDataTable(statement);
		}

		public DataTable GetAllActors()
		{
			string statement = "SELECT * FROM mudactorstate where name='id';";
			return ExecuteDataTable(statement);
		}

      //  public bool IsUserLoginValid(string shortName, string password)
      //  {
      //      DataCleaning.Sanitize(ref shortName);
      //DataCleaning.Sanitize(ref password);
      //      // Get a count of all users who match these credentials.
      //      string statement = "SELECT count(*) FROM mudactors " + 
      //  " WHERE shortname = '" + shortName + "' " +
      //  " AND userpassword = '" + password + "';";
      //      return ExecuteBooleanFromRowCount(statement);
      //  }

		/// <summary>
		/// Save the server state table and verify existence of states before writing when verify is true.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="setting"></param>
		/// <param name="verify"></param>
		public void SaveServerState(string name, string setting)
		{
			string statement;
			if(ServerStateExists(name))
			{
				statement = "UPDATE mudstate SET setting = '" + setting + "' WHERE name = '" + name + "';";
			}
			else
			{
				statement = "INSERT INTO mudstate (name, setting) VALUES ('" + name + "','" + setting + "');";
			}

			try
			{
				ExecuteNonQuery(statement);
			}
			catch 
			{}
		
		}

		public bool ServerStateExists(string name)
		{
			string statement = "SELECT count(*) FROM mudstate WHERE name='" + name + "';";
      
			int rowCount = ExecuteRowCount(statement);
			if(rowCount != 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public DataTable GetAllServerStates()
		{
			string statement = "SELECT * FROM mudstate;";
			return ExecuteDataTable(statement);
		}
	}
}
