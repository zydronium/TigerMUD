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
	/// Handles all database requests surrounding Email
	/// Activation for players.
	/// </summary>
	public interface IEmailUserActivation
	{
    ///// <summary>
    ///// Get the user's activation information.
    ///// </summary>
    ///// <param name="shortName">The user's short name.</param>
    ///// <returns>A DataTable containing the user's activation info.</returns>
    //DataTable GetUserInfo(string shortName);

    ///// <summary>
    ///// Updates the user's activation code.
    ///// </summary>
    ///// <param name="shortName">The user's short name.</param>
    ///// <param name="activationCode">The new user activation code.</param>
    //void UpdateActivationCode(string shortName, string activationCode);

    ///// <summary>
    ///// Update the user's email address.
    ///// </summary>
    ///// <param name="shortName">The user's short name.</param>
    ///// <param name="emailAddress">The new email address.</param>
    //void UpdateUserEmailAddress(string shortName, string emailAddress);

    /// <summary>
    /// Update the user's activation status.
    /// </summary>
    /// <param name="shortName">The user's short name.</param>
    /// <param name="activationStatus">The user's account activation status.</param>
    void UpdateUserActivationStatus(string shortName, bool activationStatus);
	}
}
