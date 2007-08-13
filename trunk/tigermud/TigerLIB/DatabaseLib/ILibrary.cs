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

namespace TigerMUD.DatabaseLib
{
	/// <summary>
	/// All database functions used by the Library.
	/// </summary>
	public interface ILibrary
	{
		
	/// <summary>
    /// Get all spells in the MUD
    /// </summary>
    /// <returns>A table of spells.</returns>
    DataTable GetAllSpells();

    /// <summary>
    /// Get all items in the MUD
    /// </summary>
    /// <returns>A table of items.</returns>
    DataTable GetAllActors();

   
    ///// <summary>
    ///// Checks if the user has a valid login.
    ///// I.e. if both his username and password are correct.
    ///// </summary>
    ///// <param name="shortName">The username.</param>
    ///// <param name="password">The user's password.</param>
    ///// <returns>True if the user is valid, false otherwise.</returns>
    //bool IsUserLoginValid(string shortName, 
    //  string password);

    /// <summary>
    /// Save a server state variable to the database. Adds the state
    /// to the database if it doesn't exist already.
    /// </summary>
    /// <param name="name">The name of the state variable.</param>
    /// <param name="setting">The value (state) of this variable.</param>
    void SaveServerState(string name, 
      string setting);

    /// <summary>
    /// Checks if a server state variable exists in the database or not.
    /// </summary>
    /// <param name="name">The name of state variable.</param>
    /// <returns>True if the state variable exists, false otherwise.</returns>
    bool ServerStateExists(string name);

    /// <summary>
    /// Get all server states. from the database.
    /// </summary>
    /// <returns>A table of all the server states.</returns>
    DataTable GetAllServerStates();
	}
}
