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
	/// All database functions used by Spell.
	/// </summary>
	public interface ISpell
	{

    /// <summary>
    /// Check if a setting exists.
    /// </summary>
    /// <param name="spellId">The id of the spell.</param>
    /// <param name="setting">The setting to check for.</param>
    /// <returns>True if the setting exists, false otherwise.</returns>
    bool StateExists(string spellId, string setting);

    /// <summary>
    /// Load all state settings for a spell.
    /// </summary>
    /// <param name="spellId">The id of the spell.</param>
    /// <returns>A DataTable containing all state data.</returns>
    DataTable LoadState(string spellId);

    /// <summary>
    /// Saves a state variable for a spell.
    /// </summary>
    /// <param name="spellId">The id of the spell.</param>
    /// <param name="setting">The setting to be saved.</param>
    /// <param name="settingValue">The value of the setting.</param>
        void SaveSpellState(TigerMUD.Spell spell, string setting);
	}
    
    
}
