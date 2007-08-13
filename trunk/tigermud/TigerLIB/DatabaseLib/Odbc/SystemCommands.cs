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
    /// An ODBC implementation of ISystemCommands.
    /// </summary>
    public class SystemCommands : BaseOdbc, ISystemCommands
    {
        public SystemCommands(string connectionString)
            : base(connectionString)
        {
        }

        public void AddCommand(int accessLevel, string command)
        {
            string statement = "INSERT INTO tigermud.mudcommands (actorid, type, commandname) VALUES (0, " + accessLevel + ", '" + command + "');";
            ExecuteNonQuery(statement);
        }

        public void AddCommandToDb(int accesslevel, string commandname)
        {
            string statement="SELECT * FROM mudcommands WHERE commandname='" + commandname + "';";
            DataTable dt=ExecuteDataTable(statement);
            // Check if command already exists
            if (dt.Rows.Count>0)
            {
                statement = "UPDATE mudcommands SET type = " + (int)accesslevel + " WHERE commandname = '" + commandname + "';";
            }
            else
            {
                statement = "INSERT INTO mudcommands (actorid,type,commandname) VALUES (0," + (int)accesslevel + ", '" + commandname + "');";
            }
            ExecuteNonQuery(statement);
        }


        public void DeleteCommand(string command)
        {
            string statement = "DELETE FROM mudcommands WHERE commandname = '" + command + "';";
            ExecuteNonQuery(statement);
        }

        public int GetCommandAccessLevel(string command)
        {
            string statement = "SELECT type FROM mudcommands WHERE commandname = '" + command + "';";
            object accessLevel = ExecuteScalar(statement);

            // Is the scalar value null?
            if (accessLevel == null)
            {
                return -1;
            }
            else
            {
                return Int32.Parse(accessLevel.ToString());
            }
        }
    }
}

