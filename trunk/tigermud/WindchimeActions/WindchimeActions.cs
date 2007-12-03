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
	/// Summary description for WindchimeActions.
	/// </summary>
	public class Action_windchime_push : Action
	{
		public Action_windchime_push()
		{
			name = "windchime_push";
			words = new string[1] { "push" };
		}
		
		public bool DoAction(Actor actor, Actor target, string command, string arguments)
		{
			if (actor["type"].ToString()=="user")
			{
				actor.Send( "The " + target["name"] + " tinkles merrily for a short time in response to your touch.\r\n");
                actor.Sayinroom(actor.GetNameUpper() + " touches the " + target["name"] + " and it tinkles merrily for a short time.");
			}
			else // actor is not a user
			{
				foreach(Actor tmpactor in Lib.GetWorldItems())
				{
					if( tmpactor["container"].ToString() == actor["container"].ToString() ) 
					{
                        tmpactor.Send(tmpactor.GetNameUpper() + " touches the " + target["name"] + " and it tinkles merrily for a short time.\r\n");
					}
				}
			}

            Lib.AddDelayedCommand(target, "windchime_push", "", Lib.GetTime(), 50, false);
			return true;
		}
	}
	
	
	/// <summary>
	/// Push command for windchime.
	/// </summary>
	public class Command_windchime_push : Command,ICommand
    {
       
        
        
        

		public Command_windchime_push()
		{
			name = "windchime_push";
			words = new string[1] { "push" };
			// No help, not user callable.
		}
		
		public override bool DoCommand(Actor actor, string command, string arguments)
		{
            actor.Sayinroom("The " + actor["name"] + " tinkles one last time and stops moving.");
			return true;
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public string[] Words
        {
            get
            {
                return words;
            }
        }
        public HelpInfo Help
        {
            get
            {
                return help;
            }
        }
	}
}
