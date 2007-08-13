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
	/// Defines all database operations relating to the internal
	/// bug tracker.
	/// </summary>
	public interface IMudBugs
	{
    /// <summary>
    /// Adds a new bug to the database.
    /// </summary>
    /// <param name="userShortName">The user adding the bug.</param>
    /// <param name="bugText">The bug text.</param>
    void AddBug(string userShortName, string bugText);

    /// <summary>
    /// Gets a list of unread bugs.
    /// </summary>
    /// <param name="includeReadBugs">True if the search must return previously read bugs, false otherwise.</param>
    /// <returns>A list of bugs.</returns>
    DataTable GetBugList(bool includeReadBugs);

    /// <summary>
    /// Marks a bug as read.
    /// </summary>
    /// <param name="bugId">The bug ID</param>
    void MarkBugAsRead(int bugId);

    /// <summary>
    /// Clears all bugs
    /// </summary>
    /// <param name="allBugs">True if you want to clear read and unread bugs, 
    /// false if you only want to clear read bugs.</param>
    void ClearBugs(bool allBugs);
	}
}
