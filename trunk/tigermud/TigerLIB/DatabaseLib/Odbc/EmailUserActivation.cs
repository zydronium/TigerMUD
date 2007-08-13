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
using System.Data.Odbc;
using TigerMUD.DatabaseLib;

namespace TigerMUD.DatabaseLib.Odbc
{
	/// <summary>
	/// An implementation of Email User Activation in terms of 
	/// ODBC connectivity.
	/// </summary>
	public class EmailUserActivation : BaseOdbc, IEmailUserActivation
	{
		public EmailUserActivation(string connectionString) : base (connectionString)
		{
    }

    //public DataTable GetUserInfo(string shortName)
    //{
    //  string statement = "SELECT email, activated, activationcode from mudactors where shortname = '" + shortName + "';";    
    //  DataTable outputTable = ExecuteDataTable(statement);

    //  // Set the activated boolean if it's null
    //  if(outputTable.Rows[0]["activated"] == null)
    //  {
    //    outputTable.Rows[0]["activated"] = false;
    //  }

    //  return outputTable;
    //}

    //public void UpdateActivationCode(string shortName, string activationCode)
    //{

    //  string statement = "UPDATE mudactors SET activationcode = '" + activationCode + 
    //                     "' WHERE shortname = '" + shortName + "';";
    //  base.ExecuteNonQuery(statement);
    //}

    //public void UpdateUserEmailAddress(string shortName, string emailAddress)
    //{
    //  string statement = "UPDATE mudactors SET email = '" + emailAddress + 
    //                     "' WHERE shortname = '" + shortName + "';";
    //  base.ExecuteNonQuery(statement);
    //}

    public void UpdateUserActivationStatus(string shortName, bool activationStatus)
    {
      string statement = "UPDATE mudactors SET activated = true WHERE shortname = '" + shortName + "';";
      base.ExecuteNonQuery(statement);
    }
  }
}
