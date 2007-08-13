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
	/// All database functions related to Internal Mail
	/// </summary>
	public interface IInternalMail
	{
		/// <summary>
		/// Get all messages sent to a user.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <returns>A table of messages.</returns>
		DataTable GetAllMessages(string shortName);

		/// <summary>
		/// Get all unread messages sent to a user.
		/// </summary>
		/// <param name="shortName">The user's short name.</param>
		/// <returns>A table of unread messages.</returns>
		DataTable GetAllUnreadMessages(string shortName);

		/// <summary>
		/// Get all messages sent by a user.
		/// </summary>
		/// <param name="shortName">The user's short name</param>
		/// <returns>A table of messages sent by the user.</returns>
		DataTable GetAllSentMessages(string shortName);


		/// <summary>
		/// Mark an email message as read.
		/// </summary>
		/// <param name="mailID"></param>
		void MarkAsRead(int mailID);

		/// <summary>
		/// Sends mail to a user.
		/// </summary>
		/// <param name="M_Sender"></param>
		/// <param name="M_Receiver"></param>
		/// <param name="M_Subject"></param>
		/// <param name="M_Body"></param>
		/// <returns></returns>
		bool SendMessage(string M_Sender, string M_Receiver, string M_Subject, string M_Body);

	}
}
