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
using System.Collections;
using System.Data.Odbc;
using System.Data;
using System.Net.Sockets;
using System.Net;
using TigerMUD.CommsLib;


namespace TigerMUD
{
	public class Commandprocessor
	{    
		public bool processcommand(IUserSocket userSocket, string messagefrom, string playercommand, bool login)
		{ 
			Actor user = new Actor(); // Current user
			Actor chkuser = new Actor(); // Temp array of users for search loops
			object chkobject = new object();
			ArrayList chkobjects = new ArrayList();
			Actor chkmob = new Actor();
			Actor room = new Actor(); // Current room during item search loops
			Actor item = new Actor(); // Current item during item search loops       
			Actor item2 = new Actor(); // Temp variable for trading
			Actor chkitem = new Actor(); // Temp variable for scanning item arraylist
            //int maxmapx=75; // Game map is limited to a width of this number
            //int maxmapy=19; // Game map is limited to a height of this number
            //string[,] mapgrid=new string[maxmapx,maxmapy];
            string message = null;
            string clientip = null;
                     
            
			// Here's where we get the first command from a newly logged in user
			// When login bit is set, this is a user login event
			if(login) 
			{
				// Get instance of this user from the name
				user=Lib.GetByName(playercommand.ToLower());
				user.UserSocket = userSocket;
				user["connected"]=true;
                // clear combat
                user["target"] = "";

                if (Lib.ConvertToBoolean(user["lockedout"]))
				{
					user.SendAnnouncement("You have been locked out. Please contact the MUD administrator.");
					return false;
				}

                Lib.log.Add(user.UserSocket, user["shortname"].ToString(), "LOGIN (" + user["shortname"].ToString() + ") (" + userSocket.ClientEndpointId + ") Online:" + Lib.connections);
				user.Send( Lib.Ansifboldred + "\r\nWelcome " + user.GetNameUpper() + "!\r\n");
				
				// Set login time for calculating total played time
				user["loginticks"]=Lib.GetTime();
				// Display room
				user.Showroom();
				// Set idle disconnect timer
				user["lastmessageticks"]=Lib.GetTime();
				// Send 'he arrived' messages if someone is in the room where the user appears
                user.Sayinroom(user.GetNameUpper() + " appears from nowhere.");
      
				// Notify his friends that he has arrived
				ShowWelcomeMessages(user);

				user.Showprompt();
				return true;
			}

			
			
			// Most importantly, set the context of user to the user who sent this message 
			for (int i=Lib.actors.Count-1;i>=0;i--)
			{
				Actor tmpuser=(Actor)Lib.actors[i];
                if (tmpuser["shortname"].ToString() == messagefrom) 
				{ 
					user=tmpuser;
					break;
				}
			}

            // Log player commands
            message += DateTime.Now;
            if (user["name"] != null)
            {
                message += " " + user["name"].ToString();
            }
            if (user.UserSocket != null)
            {
                clientip = user.UserSocket.ClientEndpointId;
                message += " from " + clientip;
            }
            if (playercommand != null)
            {
                message += " command: " + playercommand;
            }
            Lib.commandlogwriter.WriteLine(message);
			
			// This counts as activity, so update timer for idle disconnect
			user["lastmessageticks"]=Lib.GetTime();
			
            // If no text at all, then try to run the more command.
            if( playercommand == "" ) 
            {
                user.RunCommand("more", null);
                user.Showprompt();
                return true;
            }

			// Store current message for the "again" command to repeat it
			// But don't store the "again" command itself AS the last command
			// and don't store anything if the user just logged in
			if(!playercommand.StartsWith(".") && playercommand != "again" && !login) 
			{
				user["lastmessagefrom"]=messagefrom;
				user["lastmessage"]=playercommand;
			}

			//Parse command word from arguments
			string[] splitcommand = Lib.SplitCommand(playercommand);
			string playerword = splitcommand[0];
			string arguments = splitcommand[1];
			playerword=playerword.ToLower();
			Command command = (Command)user.GetCommandByWord(playerword);
			if (command != null)
			{
                try
                {
                    command.DoCommand(user, playerword, arguments.Trim());
                }
                catch (Exception ex)
                {
                    if (command != null) Lib.PrintLine(DateTime.Now + " EXCEPTION running command: " + command.Name + ": " + ex.Message + ex.StackTrace);
                    else Lib.PrintLine(DateTime.Now + " EXCEPTION running a command: " + ex.Message + ex.StackTrace);
                    return false;
                }
			}
			else
			{
				// if we got here, the command was not found.
				user.SendError("Unknown command, type 'help' for a list of commands.\r\n");
				user.Showprompt();
				return true;
			}
			if (command.Name!="command_again")
			{
				// Prevent double command prompts when using the 'again' command
				user.Showprompt();
			}
			return true;
		}

		/// <summary>
		/// Show all welcome messages for this user.
		/// </summary>
		/// <param name="user">The user to send the messages to.</param>
		private void ShowWelcomeMessages(Actor user)
		{
			// Notify all the user's friends
			NotifyFriends(user);

			// Show the last login time
			ShowLastLoginTime(user);

            //Update last login date/time and ip
            user["lastlogindate"] = System.DateTime.Now.ToString();
            user["lastloginip"] = user.userSocket.ClientEndpointId;

		}

		/// <summary>
		/// Let this user's friends know that he has just logged in. Also lets
		/// the user know that his friends are online.
		/// </summary>
		/// <param name="user">The user.</param>
		private void NotifyFriends(Actor user)
		{
            DataTable friends = Lib.dbService.Actor.GetFriendsList(user["shortname"].ToString());
			int friendCount = 0;
			string friendNames = "";

			foreach(DataRow row in friends.Rows)
			{
				if((bool)row["IsAuthorised"])
				{
                    Actor friendUser = Lib.Checkonline(row["FriendName"].ToString());
					if(friendUser != null)
					{
						friendUser.SendAlertGood(user["shortname"] + " has just logged on.\r\n");
						friendCount++;
						friendNames += friendUser["shortname"] + ", ";
					}
				}
			}

			// Are any of this user's friends online?
			if(friendCount > 0)
			{
				// Remove the last ", "
				friendNames = friendNames.Remove(friendNames.Length - 2, 2);
				user.SendAlertGood(friendCount + " friend(s) online: " + friendNames + "\r\n");
			}
		}

		/// <summary>
		/// Show the user's last login time and IP address.
		/// </summary>
		/// <param name="user">The user to get the details for.</param>
		public static void ShowLastLoginTime(Actor user)
		{
			// Show the last login time.
			user.SendSystemMessage("You last logged in on " + user["lastlogindate"] + 
					" from " + user["lastloginip"] + ".\r\n");
		}
	}
}
