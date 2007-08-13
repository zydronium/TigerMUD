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
    /// An ODBC implementation of ISpell.
    /// </summary>
    public class Spell : BaseOdbc, ISpell
    {
        public Spell(string connectionString)
            : base(connectionString)
        {}
        
        public bool StateExists(string spellId, string setting)
        {
            string statement = "SELECT count(*) FROM mudspellstate WHERE name='" + setting + "' AND id='" + spellId + "';";
            return ExecuteBooleanFromRowCount(statement);
        }

        public DataTable LoadState(string spellId)
        {
            string statement = "SELECT * FROM mudspellstate WHERE id='" + spellId + "';";
            return ExecuteDataTable(statement);
        }

        public void SaveSpellState(TigerMUD.Spell spell, string setting)
        {
            string statement;
            //string cleaned_state = spell[setting].ToString();
            //string cleaned_state = DataCleaning.Sanitize(spell[setting].ToString());
            // Check if state exists
            if (StateExists(spell["id"].ToString(), setting))
            {
                statement = "UPDATE mudspellstate SET setting = '" + spell[setting].ToString() + "', datatype='" + (spell[setting]).GetType().ToString() + "' WHERE name = '" + setting + "' AND id='" + spell["id"] + "';";
            }
            else
            {
                statement = "INSERT INTO mudspellstate (id,name, setting, datatype) VALUES ('" + spell["id"] + "', '" + setting + "', '" + spell[setting].ToString() + "', '" + (spell[setting]).GetType().ToString() + "');";
            }
            ExecuteNonQuery(statement);

        }

    }
}
