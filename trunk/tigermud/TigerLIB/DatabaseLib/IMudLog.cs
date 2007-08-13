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
	/// Handles the data for MUD logging activities 
	/// </summary>
	public interface IMudLog
	{
    /// <summary>
    /// Log a message.
    /// </summary>
    /// <param name="message">The message content.</param>
    /// <param name="clientIPAddress">The user's IP address (if applicable).</param>
    /// <param name="shortName">The user's short name (if applicable).</param>
    void Log(string message, string clientIPAddress, string shortName);

    /// <summary>
    /// Get all the log entries.
    /// </summary>
    /// <returns></returns>
    DataTable GetAllEntries();

    /// <summary>
    /// Clear all the entries in the log.
    /// </summary>
    /// <returns></returns>
    void ClearLog();
	}
}
