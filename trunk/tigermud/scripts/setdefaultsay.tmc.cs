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
    /// Speechmode command for player.
    /// </summary>
    public class Command_setdefaultsay : Command
    {
        public Command_setdefaultsay()
        {
            name = "command_setdefaultsay";
            words = new string[1] { "defaultsay" };
            help.Command = "defaultsay";
            help.Summary = "sets your speech mode when speaking to other players.";
            help.Syntax = "defaultsay <mode>";
            help.Examples = new string[1];
            help.Examples[0] = "defaultsay shout";
            //			help.Description = "Long description here.";
        }

        public override bool DoCommand(Actor actor, string command, string arguments)
        {
            string verb = "";

            // Set the mode if the user specified a parameter
            if (arguments != "")
            {
                try
                {
                    arguments = arguments.Trim();
                    actor["defaultsay"] = arguments;
                }
                catch
                {
                    actor.SendError("An error occurred setting your speech mode. Returning to 'say' as the default.\r\n");
                    actor["defaultsay"] = "say";
                    return false;
                }
            }

            // Report to the user what the speech mode is
            switch (actor["defaultsay"].ToString())
            {
                case "say":
                    verb = "say";
                    break;
                case "yell":
                    verb = "yell";
                    break;
                case "shout":
                    verb = "shout";
                    break;
                case "whisper":
                    verb = "whisper";
                    break;
                case "lecture":
                    verb = "lecture";
                    break;
                case "think":
                    verb = "think";
                    break;
                default:
                    verb = "say";
                    break;
            }

            actor.Send("Your speech mode is '" + verb + "'." + "\"\r\n");
            return true;
        }
    }
	
}
