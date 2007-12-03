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
    /// ODBC implementation of IActor.
    /// </summary>
    public class Actor : BaseOdbc, IActor
    {
        public Actor(string connectionString)
            : base(connectionString)
        { }

        /// <summary>
        /// Check the database if an actor exists with this ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(string id)
        {
            //string statement = "SELECT count(*) FROM mudactors WHERE id='" + id + "';";
            //string statement2 = "SELECT count(*) FROM mudactors WHERE shortname='" + id + "';";

            string statement = "SELECT count(*) FROM mudactorstate WHERE id='" + id + "';";
            string statement2 = "SELECT count(*) FROM mudactorstate WHERE name='shortname' and setting='" + id + "';";

            // If user either exists by id or name then return true
            return (ExecuteBooleanFromRowCount(statement) || ExecuteBooleanFromRowCount(statement2));
        }


        public string GetCommand(string actorId, int type)
        {
            string statement = "SELECT commandname FROM mudcommands WHERE type=" + type + " AND actorid='" + actorId + "';";
            object command = ExecuteScalar(statement);

            // Is the resulting scalar null?
            if (command == null)
            {
                return null;
            }
            else
            {
                return command.ToString();
            }
        }

        public string GetAction(string actorId, int type)
        {
            string statement = "SELECT actionname FROM mudactions WHERE type=" + type + " AND actorid='" + actorId + "';";
            object action = ExecuteScalar(statement);

            // Is the resulting scalar null?
            if (action == null)
            {
                return null;
            }
            else
            {
                return action.ToString();
            }
        }


        public bool StateExists(string userId, string setting)
        {
            string statement = "SELECT count(*) FROM mudactorstate WHERE name='" + setting + "' AND id='" + userId + "';";
            return ExecuteBooleanFromRowCount(statement);
        }

        public DataTable LoadActorState(string userId)
        {
            string statement = "SELECT * FROM mudactorstate WHERE id='" + userId + "';";
            return ExecuteDataTable(statement);
        }

        public DataTable RunQuery(string statement)
        {
            try
            {
                return ExecuteDataTable(statement);
            }
            catch 
            {
                return null;
            }
        }

        public string RunNonQuery(string statement)
        {
            try
            {
                ExecuteNonQuery(statement);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message + ex.StackTrace;
            }

        }

        public void SaveActorState(TigerMUD.Actor actor, string setting)
        {
            string statement;
            string setvalue=actor[setting].ToString();
            //string cleaned_state = actor[setting].ToString();
            //string cleaned_state = DataCleaning.Sanitize(actor[setting].ToString());
            // Check if state exists
            if (StateExists(actor["id"].ToString(), setting))
            {
                if (setting.ToLower() == "lastmessage")
                {
                    // Case where user input might get saved direct to DB, so clean it up carefully
                    // first remove any apostrophies instead of escaping them
                    setvalue = setvalue.Replace(@"'", "");
                    // Then do a flat sanitize as well
                    DatabaseLib.DataCleaning.Sanitize(setvalue);
                    // Now we can use it
                }
                statement = "UPDATE mudactorstate SET setting = '" + setvalue + "', datatype='" + (actor[setting]).GetType().ToString() + "' WHERE name = '" + setting + "' AND id='" + actor["id"] + "';";
            }
            else
            {
                statement = "INSERT INTO mudactorstate (id,name, setting, datatype) VALUES ('" + actor["id"] + "', '" + setting + "', '" + actor[setting].ToString() + "', '" + (actor[setting]).GetType().ToString() + "');";
            }
            ExecuteNonQuery(statement);
        }
             
        public void DeleteActor(string id)
        {
            string statement = "DELETE * FROM mudactorstate WHERE id='" + id + "';";
            ExecuteNonQuery(statement);
        }
        
        public void AddFriend(string shortName, string friendName)
        {
            string statement = "INSERT INTO muduserfriends VALUES ('"
                + shortName + "', '" + friendName + "', " + Convert.ToByte(false) + ");";
            ExecuteNonQuery(statement);
        }

        public void AddFriendWithAuthorisation(string shortName,
            string friendName)
        {
            string statement = "INSERT INTO muduserfriends VALUES ('"
                + shortName + "', '" + friendName + "', " + Convert.ToByte(true) + ");";
            ExecuteNonQuery(statement);
        }

        public bool IsFriendInList(string shortName, string friendName)
        {
            string statement = "SELECT count(muduserfriends.ShortName) " +
                " FROM muduserfriends " +
                " WHERE (((muduserfriends.ShortName)='" + shortName + "') " +
                " AND ((muduserfriends.FriendName)='" + friendName + "') " +
                " AND ((muduserfriends.IsAuthorised)=True));";

            return ExecuteBooleanFromRowCount(statement);
        }

        public bool IsFriendPendingAuthorisation(string shortName, string friendName)
        {
            string statement = "SELECT count(muduserfriends.ShortName) " +
                " FROM muduserfriends " +
                " WHERE (((muduserfriends.ShortName)='" + shortName + "') " +
                " AND ((muduserfriends.FriendName)='" + friendName + "') " +
                " AND ((muduserfriends.IsAuthorised)=False));";

            return ExecuteBooleanFromRowCount(statement);
        }

        public void AuthoriseFriend(string shortName, string requestorName)
        {
            string statement = "UPDATE muduserfriends SET IsAuthorised = " + Convert.ToByte(true) + " " +
                " WHERE ShortName = '" + requestorName + "' " +
                " AND FriendName = '" + shortName + "';";

            ExecuteNonQuery(statement);
        }

        public void RemoveFriend(string shortName, string friendName)
        {
            string statement = "DELETE FROM muduserfriends " +
                " WHERE ShortName = '" + shortName + "' " +
                " AND FriendName = '" + friendName + "';";

            ExecuteNonQuery(statement);
        }

        public void RejectFriendRequest(string shortName, string requestorName)
        {
            string statement = "DELETE FROM muduserfriends " +
                " WHERE (((muduserfriends.ShortName)='" + requestorName + "') " +
                " AND ((muduserfriends.FriendName)='" + shortName + "') " +
                " AND ((muduserfriends.IsAuthorised)=False));";

            ExecuteNonQuery(statement);
        }

        public DataTable GetPendingAuthorisationRequests(string shortName)
        {
            string statement = "SELECT DISTINCT  muduserfriends.ShortName " +
                " FROM muduserfriends " +
                " WHERE (((muduserfriends.FriendName)='" + shortName + "') " +
                " AND ((muduserfriends.IsAuthorised)=False));";

            return ExecuteDataTable(statement);
        }

        public DataTable GetFriendsList(string shortName)
        {
            string statement = "SELECT FriendName, IsAuthorised FROM muduserfriends " +
                " WHERE ShortName = '" + shortName + "';";

            return ExecuteDataTable(statement);
        }

        public bool IsActorWaitingForAuthorisation(string shortName, string requestorName)
        {
            string statement = "SELECT count(*) from muduserfriends " +
                " WHERE (((muduserfriends.ShortName)='" + requestorName + "') " +
                " AND ((muduserfriends.FriendName)='" + shortName + "') " +
                " AND ((muduserfriends.IsAuthorised)=False));";

            return ExecuteBooleanFromRowCount(statement);
        }

    }
}
