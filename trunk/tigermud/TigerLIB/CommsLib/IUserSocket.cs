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

namespace TigerMUD.CommsLib 
{
	/// <summary>
	/// Describes an interface used to communicate with a user
	/// connected to the MUD.
	/// </summary>
	public interface IUserSocket 
	{
    /// <summary>
    /// Gets the client's newline character.
    /// </summary>
    /// <returns>A string representing the newline character.</returns>
    string GetNewLineCharacter();

    /// <summary>
    /// Sends a message to the client.
    /// </summary>
    /// <param name="text">The message to send to the client.</param>
    void Send(string text);

    /// <summary>
    /// Sends a message with a newline to the client.
    /// </summary>
    /// <param name="text">The message to send to the client.</param>
    void SendLine(string text);

    /// <summary>
    /// Gets a response from the client.
    /// </summary>
    /// <returns>The response.</returns>
    string GetResponse();

    /// <summary>
    /// Closes the user socket and disconnects the client.
    /// </summary>
    void Close();

    /// <summary>
    /// Gets the connection status of the user socket.
    /// </summary>
    bool Connected
    {
      get;
    }

    /// <summary>
    /// Gets the client's endpoint ID
    /// </summary>
    /// <remarks>
    /// This could be an IP address for a tcp connected client
    /// or an email address or sign-on name for an MSN connected
    /// client.
    /// </remarks>
    string ClientEndpointId
    {
      get;
    }

    /// <summary>
    /// Gets this user socket's unique ID (guid)
    /// </summary>
    string UniqueId
    {
      get;
    }
	}

  public delegate void ClientDisconnectedEventHandler(ClientListener sender);
}
