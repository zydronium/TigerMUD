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
	/// An ODBC implementation of IProfanityFilter.
	/// </summary>
	public class ProfanityFilter : BaseOdbc, IProfanityFilter
	{
		public ProfanityFilter(string connectionString) : base(connectionString)
		{
    }

    public void AddProfanity(string profanity)
    {
      string statement = "INSERT INTO mudprofanities(profanity) VALUES ('" + profanity + "');";
      ExecuteNonQuery(statement);
    }

    public void DeleteProfanity(string profanity)
    {
      string statement = "DELETE FROM mudprofanities WHERE profanity = '" + profanity + "';";
      ExecuteNonQuery(statement);
    }

    public string[] GetProfanityList()
    {
      // Get the list of profanities from the database
      string statement = "SELECT * FROM mudprofanities;";
      DataTable list = ExecuteDataTable(statement);

      // Put the profanities in a string array.
      string[] words = new string[list.Rows.Count];
      for(int counter = 0; counter < words.Length; counter++)
      {
        words[counter] = list.Rows[counter][0].ToString();
      }

      return words;
    }
  }
}
