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
	/// Handles all database operations for an Actor.
	/// </summary>
	public interface IActor
	{
            /// <summary>
        /// Check the database if an actor exists with this ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(string id);

		/// <summary>
		/// Gets a command name for an actor.
		/// </summary>
		/// <param name="actorId">The actor's ID.</param>
		/// <param name="type">The command type.</param>
		/// <returns>The command name.</returns>
		string GetCommand(string actorId, int type);

		/// <summary>
		/// Gets an action name for an actor.
		/// </summary>
		/// <param name="actorId">The actor's ID.</param>
		/// <param name="type">The action type.</param>
		/// <returns>The action name.</returns>
		string GetAction(string actorId, int type);
	

		/// <summary>
		/// Check if a state exists for the user.
		/// </summary>
		/// <param name="userId">The id of the user.</param>
		/// <param name="setting">The setting to check for.</param>
		/// <returns>True if the state exists, false otherwise.</returns>
		bool StateExists(string userId, string setting);

		/// <summary>
		/// Load all state variables for the user.
		/// </summary>
		/// <param name="userId">The id of the user.</param>
		/// <returns>A table containing all the state data.</returns>
		DataTable LoadActorState(string userId); 

		/// <summary>
		/// Save a state setting for a user.
		/// </summary>
		/// <param name="userId">The id of the user.</param>
		/// <param name="setting">The setting name.</param>
		/// <param name="settingValue">The value of the setting.</param>
		void SaveActorState(Actor actor, string setting);

        /// <summary>
        /// Runs a raw sql query. WARNING! If you don't know sql statements, then stay away.
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        DataTable RunQuery(string statement);


        /// <summary>
        /// Runs a raw sql insert/update. WARNING! If you don't know sql statements, then stay away.
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        string RunNonQuery(string statement);


		void DeleteActor(string id);
		


		/// <summary>
		/// Add a new friend, with a pending authorisation.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="friendName">The friend's name.</param>
		void AddFriend(string shortName, string friendName);

		/// <summary>
		/// Add a friend and set the user to authorised immediately.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="friendName">The friend's name.</param>
		void AddFriendWithAuthorisation(string shortName, string friendName);

		/// <summary>
		/// Checks if a friend is in the list and is authorised.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="friendName">The friend's short name.</param>
		/// <returns>True if the friend is in the list and authorised, false otherwise.</returns>
		bool IsFriendInList(string shortName, string friendName);

		/// <summary>
		/// Checks if a friend has not authorised himself to be in
		/// this user's list.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="friendName">The friend's short name.</param>
		/// <returns>True if the friend has not authorised himself to be in the list, false otherwise.</returns>
		bool IsFriendPendingAuthorisation(string shortName, string friendName);

		/// <summary>
		/// Authorise a requestor to become a friend.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="requestorName">The requestor's name.</param>
		void AuthoriseFriend(string shortName, string requestorName);

		/// <summary>
		/// Remove a friend from the list.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="friendName">The friend's name.</param>
		void RemoveFriend(string shortName, string friendName);

		/// <summary>
		/// Rejects a request to be on someone's friends list.
		/// </summary>
		/// <param name="shortName">The person rejecting the request.</param>
		/// <param name="requestorName">The requestor.</param>
		void RejectFriendRequest(string shortName, string requestorName);

		/// <summary>
		/// Get a list of people who want this user to be on their
		/// friends list.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <returns>A table of pending requests.</returns>
		DataTable GetPendingAuthorisationRequests(string shortName);

		/// <summary>
		/// Get friends list.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <returns>A list of friends.</returns>
		DataTable GetFriendsList(string shortName);

		/// <summary>
		/// Check if someone is waiting on this user for authorisation 
		/// to be on his friends list.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <param name="requestorName">The user requesting authorisation.</param>
		/// <returns>True if the requestor is waiting, false otherwise.</returns>
		bool IsActorWaitingForAuthorisation(string shortName, string requestorName);

		
	}
}
