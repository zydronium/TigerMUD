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

namespace TigerMUD
{
    /// <summary>
    /// Say command for player.
    /// </summary>
    public class Command_say : Command
    {
        public Command_say()
        {
            name = "command_say";
            words = new string[1] { "say" };
            help.Command = "say";
            help.Summary = "sends a message to all the awake players in your current room.";
            help.Syntax = "say <text>";
            help.Examples = new string[1];
            help.Examples[0] = "say Hello, how are you?";
            //			help.Description = "Long description here.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            if (arguments.Length < 1)
            {
                actor.SendError("You must specify text to say.\r\n");
                return false;
            }
            actor.Send("You say, \"" + arguments + "\"\r\n");
            actor.Sayinroom(actor["shortnameupper"] + " says, '" + arguments + "'");
            return true;
        }
    }

}
